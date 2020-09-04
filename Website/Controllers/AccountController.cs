﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Website.Classes;
using DataAccess.Models;
using Website.Repositories;
using Website.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Customer> userManager;
        private readonly IConfiguration configuration;
        private readonly IUnitOfWork unitOfWork;

        public AccountController(UserManager<Customer> userManager, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.unitOfWork = unitOfWork;
        }



        // ..................................................................................Register.....................................................................
        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult> Register(Account account)
        {
            Customer customer = account.CreateCustomer();

            // Add the new customer to the database
            IdentityResult result = await userManager.CreateAsync(customer, account.Password);


            if (result.Succeeded)
            {
                // Create the new list and add it to the database
                List newList = new List
                {
                    //Id = Guid.NewGuid().ToString("N").ToUpper(),
                    Name = "Wish List",
                    Description = string.Empty,
                    CollaborateId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
                };

                unitOfWork.Lists.Add(newList);


                // Set the owner as the first collaborator of the list
                ListCollaborator collaborator = new ListCollaborator
                {
                    //Id = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                    CustomerId = customer.Id,
                    ListId = newList.Id,
                    IsOwner = true
                };

                unitOfWork.Collaborators.Add(collaborator);


                // Save all updates to the database
                await unitOfWork.Save();


                // The new customer was successfully added to the database
                return Ok();
            }
            else
            {
                string error = string.Empty;

                if (result.Errors.Count(x => x.Code == "DuplicateEmail") == 1)
                {
                    error = "The email address, \"" + account.Email.ToLower() + ",\" already exists with another Niche Shack account. Please use another email address.";
                }

                return Conflict(error);
            }
        }





        // ..................................................................................Sign In.....................................................................
        [HttpPost]
        [Route("SignIn")]
        public async Task<ActionResult> SignIn(SignIn signIn)
        {
            // Get the customer from the database based on the email address
            Customer customer = await userManager.FindByEmailAsync(signIn.Email);


            // If the customer is in the database and the password is valid, create claims for the access token
            if (customer != null && await userManager.CheckPasswordAsync(customer, signIn.Password))
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim("acc", "customer"),
                    new Claim(ClaimTypes.NameIdentifier, customer.Id),
                    new Claim(JwtRegisteredClaimNames.Iss, configuration["TokenValidation:Site"]),
                    new Claim(JwtRegisteredClaimNames.Aud, configuration["TokenValidation:Site"]),
                    new Claim("isPersistent", signIn.IsPersistent.ToString())
                };

                // Return with the token data
                return Ok(await GenerateTokenData(customer, claims));
            }

            return Conflict("Your password and email do not match. Please try again.");
        }




        // ..................................................................................Update Customer Name.....................................................................
        [HttpPut]
        [Route("UpdateName")]
        [Authorize(Policy = "Account Policy")]
        public async Task<ActionResult> UpdateCustomerName(UpdatedCustomerName updatedCustomerName)
        {
            // Get the customer from the database based on the customer id from the claims via the access token
            Customer customer = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // If the customer is found, update his/her name
            if (customer != null)
            {
                customer.FirstName = updatedCustomerName.FirstName;
                customer.LastName = updatedCustomerName.LastName;

                // Update the name in the database
                IdentityResult result = await userManager.UpdateAsync(customer);

                if (result.Succeeded)
                {
                    return Ok();
                }
            }


            return BadRequest();
        }








        // ..................................................................................Update Email.....................................................................
        [HttpPut]
        [Route("UpdateEmail")]
        [Authorize(Policy = "Account Policy")]
        public async Task<ActionResult> UpdateEmail(UpdatedEmail updatedEmail)
        {
            // Get the customer from the database based on the customer id from the claims via the access token
            Customer customer = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);


            // If the customer is found...
            if (customer != null)
            {
                if(!await userManager.CheckPasswordAsync(customer, updatedEmail.Password))
                {
                    return Conflict("Your password and current email do not match.");
                }

                // Update the new email in the database
                IdentityResult result = await userManager.SetEmailAsync(customer, updatedEmail.Email);


                // If the update was successful, return ok
                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    string error = string.Empty;

                    if(result.Errors.Count(x => x.Code == "DuplicateEmail") == 1)
                    {
                        error = "The email address, \"" + updatedEmail.Email.ToLower() + ",\" already exists with another Niche Shack account. Please use another email address.";
                    }

                    return Conflict(error);
                }
            }

            return BadRequest();
        }








        // ..................................................................................Update Password.....................................................................
        [HttpPut]
        [Route("UpdatePassword")]
        [Authorize(Policy = "Account Policy")]
        public async Task<ActionResult> UpdatePassword(UpdatedPassword updatedPassword)
        {
            // Get the customer from the database based on the customer id from the claims via the access token
            Customer customer = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // If the customer is found...
            if (customer != null)
            {
                // Update the password in the database
                IdentityResult result = await userManager.ChangePasswordAsync(customer, updatedPassword.CurrentPassword, updatedPassword.NewPassword);


                // If the password was successfully updated, return ok
                if (result.Succeeded)
                {
                    return Ok();
                }
            }

            return Conflict();
        }



        // ..................................................................................Update Profile Picture.....................................................................
        [HttpPost, DisableRequestSizeLimit]
        [Route("UpdateProfilePicture")]
        public async Task<ActionResult> UpdateImage()
        {

            // Get the new image
            IFormFile imageFile = Request.Form.Files["image"];

            Bitmap bmp;
            StringValues width;
            StringValues height;
            StringValues scale;



            Request.Form.TryGetValue("width", out width);
            Request.Form.TryGetValue("height", out height);
            Request.Form.TryGetValue("scale", out scale);



            decimal originalWidth = Convert.ToDecimal(width);
            decimal originalHeight = Convert.ToDecimal(height);
            decimal scaleValue = Convert.ToDecimal(scale);




            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                using (var img = Image.FromStream(memoryStream))
                {

                    bmp = new Bitmap(img);
                    
                }


            }


           



            
            var scaleWidth = (int)(originalWidth * scaleValue);
            var scaleHeight = (int)(originalHeight * scaleValue);
            var scaledBitmap = new Bitmap(scaleWidth, scaleHeight);


            Graphics graph = Graphics.FromImage(scaledBitmap);

            graph.DrawImage(bmp, 0, 0, scaleWidth, scaleHeight);



            Bitmap crpImg = new Bitmap(300, 300);

            for (int i = 0; i < 300; i++)
            {
                for (int y = 0; y < 300; y++)
                {
                    Color pxlclr = scaledBitmap.GetPixel(22 + i, 22 + y);
                    crpImg.SetPixel(i, y, pxlclr);
                }
            }



            string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");
            string filePath = Path.Combine(imagesFolder, "willow.png");

            crpImg.Save(filePath, ImageFormat.Png);


            return Ok();
        }




        private async Task<string> CopyImage(IFormFile imageFile)
        {
            // This will get the file extension
            Regex regex = new Regex(@"\.(jpg|jpeg|gif|png|bmp|tiff|tga|svg|webp)$", RegexOptions.IgnoreCase);
            Match match = regex.Match(imageFile.FileName);
            string fileExtension = match.Value;


            // Create a new unique name for the image
            string imageUrl = Guid.NewGuid().ToString("N") + fileExtension;

            // Place the new image into the images folder
            string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");
            string filePath = Path.Combine(imagesFolder, imageUrl);

            // Create the file stream
            var fileStream = new FileStream(filePath, FileMode.Create);

            // Copy to image to the images folder
            await imageFile.CopyToAsync(fileStream);


            // Close the file stream
            fileStream.Close();


            return imageUrl;
        }





        // ..................................................................................Refresh.....................................................................
        [HttpGet]
        [Route("Refresh")]
        public async Task<ActionResult> Refresh()
        {
            string accessToken = GetAccessTokenFromHeader();
            string refresh = Request.Cookies["refresh"];

            if (accessToken != null)
            {
                ClaimsPrincipal principal = GetPrincipalFromToken(accessToken);


                if (principal != null && refresh != null)
                {
                    string customerId = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

                    if (customerId != null)
                    {
                        RefreshToken refreshToken = await unitOfWork.RefreshTokens.Get(x => x.Id == refresh && x.CustomerId == customerId);
                        if (refreshToken != null)
                        {
                            // Remove the refresh token from the database
                            unitOfWork.RefreshTokens.Remove(refreshToken);
                            await unitOfWork.Save();

                            if (DateTime.Compare(DateTime.UtcNow, refreshToken.Expiration) < 0)
                            {
                                Customer customer = await userManager.FindByIdAsync(customerId);

                                // Generate a new token and refresh token
                                return Ok(await GenerateTokenData(customer, principal.Claims));
                            }
                        }
                    }
                }
            }


            return Ok();
        }






        // ..................................................................................Sign Out.....................................................................
        [HttpGet]
        [Route("SignOut")]
        public async Task<ActionResult> SignOut()
        {
            string refresh = Request.Cookies["refresh"];

            if (refresh != null)
            {
                RefreshToken refreshToken = await unitOfWork.RefreshTokens.Get(x => x.Id == refresh);

                if (refreshToken != null)
                {
                    unitOfWork.RefreshTokens.Remove(refreshToken);
                    await unitOfWork.Save();
                }

            }

            Response.Cookies.Delete("access");
            Response.Cookies.Delete("refresh");

            return NoContent();
        }





        // ..................................................................................Get Customer.....................................................................
        [HttpGet]
        [Route("GetCustomer")]
        public async Task<ActionResult> GetCustomer()
        {
            CustomerViewModel customerDTO = null;

            if (Request.Cookies["access"] != null)
            {
                Claim claim = User.FindFirst(ClaimTypes.NameIdentifier);

                if(claim != null)
                {
                    Customer customer = await userManager.FindByIdAsync(claim.Value);

                    if (customer != null)
                    {
                        customerDTO = new CustomerViewModel
                        {
                            FirstName = customer.FirstName,
                            LastName = customer.LastName,
                            Email = customer.Email,
                            Image = customer.image
                        };
                    }
                }
                
            }

            return Ok(customerDTO);
        }









        // ..................................................................................Generate Token Data.....................................................................
        private async Task<TokenData> GenerateTokenData(Customer customer, IEnumerable<Claim> claims)
        {
            // Generate the access token
            JwtSecurityToken accessToken = GenerateAccessToken(claims);

            // Generate the refresh token
            RefreshToken refreshToken = await GenerateRefreshToken(customer.Id);


            // Return the token data
            return new TokenData
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = refreshToken.Id
            };
        }








        // ..................................................................................Generate Access Token.....................................................................
        private JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims)
        {
            return new JwtSecurityToken(
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(configuration["TokenValidation:AccessExpiresInMinutes"])),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenValidation:SigningKey"])), SecurityAlgorithms.HmacSha256),
                claims: claims);
        }










        // ..................................................................................Generate Refresh Token.....................................................................
        private async Task<RefreshToken> GenerateRefreshToken(string customerId)
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);

                RefreshToken refreshToken = new RefreshToken()
                {
                    Id = Convert.ToBase64String(randomNumber),
                    CustomerId = customerId,
                    Expiration = DateTime.UtcNow.AddDays(Convert.ToInt32(configuration["TokenValidation:RefreshExpiresInDays"]))
                };

                // Add to database
                unitOfWork.RefreshTokens.Add(refreshToken);

                await unitOfWork.Save();

                return refreshToken;
            }
        }












        // ..................................................................................Get Principal From Token....................................................................
        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = configuration["TokenValidation:Site"],
                ValidIssuer = configuration["TokenValidation:Site"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenValidation:SigningKey"])),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal;

            try
            {
                principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            }
            catch (Exception)
            {

                return null;
            }


            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }



        // ..................................................................................Get Access Token From Header.....................................................................
        private string GetAccessTokenFromHeader()
        {
            StringValues value;
            Request.Headers.TryGetValue("Authorization", out value);

            if (value.Count == 0) return null;

            Match result = Regex.Match(value, @"(?:Bearer\s)(.+)");
            return result.Groups[1].Value;
        }
    }
}