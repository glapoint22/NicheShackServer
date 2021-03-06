﻿using System.Threading.Tasks;
using Manager.Repositories;
using DataAccess.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Models;
using Manager.ViewModels;
using System.Linq;
using Manager.Classes;
using System;

namespace Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }



        [HttpGet]
        public async Task<ActionResult> GetCategories()
        {
            return Ok(await unitOfWork.Categories.GetCollection<ItemViewModel<Category>>());
        }




        [HttpGet]
        [Route("Detail")]
        public async Task<ActionResult> GetCategoriesDetails()
        {
            return Ok(await unitOfWork.Categories.GetCollection(x => new
            {
                id = x.Id,
                name = x.Name,
                urlName = x.UrlName,
                icon = new
                {
                    name = x.Media.Name,
                    url = x.Media.Url
                }
            }));
        }


        [HttpGet]
        [Route("Detail/Search")]
        public async Task<ActionResult> DetailSearch(string searchWords)
        {
            return Ok(await unitOfWork.Categories.GetCollection(searchWords, x => new
            {
                id = x.Id,
                name = x.Name,
                urlName = x.UrlName,
                icon = new
                {
                    name = x.Media.Name,
                    url = x.Media.Url
                }
            }));
        }



        [HttpPut]
        public async Task<ActionResult> UpdateCategoryName(ItemViewModel category)
        {
            Category updatedCategory = await unitOfWork.Categories.Get(category.Id);

            updatedCategory.Name = category.Name;
            updatedCategory.UrlName = Utility.GetUrlName(category.Name);

            // Update and save
            unitOfWork.Categories.Update(updatedCategory);
            await unitOfWork.Save();

            return Ok();
        }





        [HttpPost]
        public async Task<ActionResult> AddCategory(ItemViewModel category)
        {
            Category newCategory = new Category
            {
                Name = category.Name,
                UrlId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                UrlName = Utility.GetUrlName(category.Name)
            };

            unitOfWork.Categories.Add(newCategory);
            await unitOfWork.Save();

            return Ok(newCategory.Id);
        }





        [HttpDelete]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            Category category = await unitOfWork.Categories.Get(id);

            unitOfWork.Categories.Remove(category);
            await unitOfWork.Save();

            return Ok();
        }





        [HttpPut]
        [Route("Image")]
        public async Task<ActionResult> UpdateCategoryImage(UpdatedProperty updatedProperty)
        {
            Category category = await unitOfWork.Categories.Get(updatedProperty.ItemId);

            category.ImageId = updatedProperty.PropertyId;

            // Update and save
            unitOfWork.Categories.Update(category);
            await unitOfWork.Save();

            return Ok();
        }




        [HttpGet]
        [Route("Image")]
        public async Task<ActionResult> GetCategoryImage(int categoryId)
        {
            return Ok(await unitOfWork.Media.Get(x => x.Id == x.Categtories.Where(y => y.Id == categoryId).Select(y => y.ImageId).FirstOrDefault(), x => new ImageViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url
            }));
        }



        [HttpGet]
        [Route("Search")]
        public async Task<ActionResult> Search(string searchWords)
        {
            return Ok(await unitOfWork.Categories.GetCollection<ItemViewModel<Category>>(searchWords));
        }
    }
}