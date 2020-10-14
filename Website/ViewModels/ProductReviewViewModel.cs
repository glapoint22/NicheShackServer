﻿using System.Collections.Generic;
using System.Linq;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace Website.ViewModels
{
    public class ProductReviewViewModel : ISelect<ProductReview, ProductReviewViewModel>, ISort<ProductReview>
    {
        private readonly string sortBy;

        public int Id { get; set; }
        public string Title { get; set; }
        public double Rating { get; set; }
        public string Username { get; set; }
        public string UserImage { get; set; }
        public string Date { get; set; }
        public bool IsVerified { get; set; }
        public string Text { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int ProductId { get; set; }



        // Constructors
        public ProductReviewViewModel() { }

        public ProductReviewViewModel(string sortBy)
        {
            this.sortBy = sortBy;
        }


        // ..................................................................................Get Reviews Per Page.....................................................................
        public int GetReviewsPerPage()
        {
            return 10;
        }





        // ..................................................................................Get Sort Options.....................................................................
        public List<KeyValuePair<string, string>> GetSortOptions()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("High to Low Rating", "high-low-rating"),
                new KeyValuePair<string, string>("Low to High Rating", "low-high-rating"),
                new KeyValuePair<string, string>("Newest to Oldest", "new-old"),
                new KeyValuePair<string, string>("Oldest to Newest", "old-new"),
                new KeyValuePair<string, string>("Most helpful", "most-helpful")
            };
        }




        // ..................................................................................Set Select.....................................................................
        public IQueryable<ProductReviewViewModel> ViewModelSelect(IQueryable<ProductReview> source)
        {
            return source.Select(x => new ProductReviewViewModel
            {
                Id = x.Id,
                Title = x.Title,
                ProductId = x.ProductId,
                Rating = x.Rating,
                Username = x.Customer.ReviewName,
                UserImage = x.Customer.Image,
                Date = x.Date.ToString("MMMM dd, yyyy"),
                IsVerified = x.Product.ProductOrders.Count(z => z.CustomerId == x.CustomerId && z.ProductId == x.ProductId) > 0,
                Text = x.Text,
                Likes = x.Likes,
                Dislikes = x.Dislikes
            });
        }




        // .............................................................................Set Sort Option.....................................................................
        public IOrderedQueryable<ProductReview> SetSortOption(IQueryable<ProductReview> source)
        {
            IOrderedQueryable<ProductReview> sortOption = null;


            switch (sortBy)
            {
                case "low-high-rating":
                    sortOption = source.OrderBy(x => x.Rating);
                    break;

                case "new-old":
                    sortOption = source.OrderByDescending(x => x.Date);
                    break;

                case "old-new":
                    sortOption = source.OrderBy(x => x.Date);
                    break;

                case "most-helpful":
                    sortOption = source.OrderByDescending(x => x.Likes);
                    break;

                default:
                    // High to low rating
                    sortOption = source.OrderByDescending(x => x.Rating);
                    break;
            }

            return sortOption;
        }
    }
}
