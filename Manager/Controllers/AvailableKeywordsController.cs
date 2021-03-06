﻿using DataAccess.Models;
using DataAccess.ViewModels;
using Manager.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailableKeywordsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public AvailableKeywordsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }


        [HttpGet]
        [Route("Groups")]
        public async Task<ActionResult> GetKeywordGroups()
        {
            IEnumerable<KeywordGroup> keywordGroups = await unitOfWork.KeywordGroups.GetCollection(x => !x.ForProduct);
            return Ok(keywordGroups
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                })
                .OrderBy(x => x.name));


        }




        [HttpGet]
        public async Task<ActionResult> GetKeywords(int groupId)
        {
            var keywords = await unitOfWork.Keywords_In_KeywordGroup.GetCollection(x => x.KeywordGroupId == groupId, x => new
            {
                id = x.Keyword.Id,
                name = x.Keyword.Name
            });
            return Ok(keywords.OrderBy(x => x.name));
        }



        [HttpPut]
        [Route("Groups")]
        public async Task<ActionResult> UpdateKeywordGroup(ItemViewModel updatedProperty)
        {
            KeywordGroup keywordGroup = await unitOfWork.KeywordGroups.Get(updatedProperty.Id);

            keywordGroup.Name = updatedProperty.Name;

            // Update and save
            unitOfWork.KeywordGroups.Update(keywordGroup);
            await unitOfWork.Save();

            return Ok();
        }






        [HttpPost]
        [Route("Groups")]
        public async Task<ActionResult> AddKeywordGroup(ItemViewModel keywordGroup)
        {


            KeywordGroup newKeywordGroup = new KeywordGroup
            {
                Name = keywordGroup.Name
            };


            // Add and save
            unitOfWork.KeywordGroups.Add(newKeywordGroup);
            await unitOfWork.Save();

            return Ok(newKeywordGroup.Id);
        }







        [HttpPost]
        public async Task<ActionResult> AddKeyword(ItemViewModel item)
        {
            Keyword newKeyword = new Keyword
            {
                Name = item.Name.Trim().ToLower()
            };


            int keywordId = await unitOfWork.Keywords.Get(x => x.Name == newKeyword.Name, x => x.Id);




            // Add and save
            if (keywordId == 0)
            {
                unitOfWork.Keywords.Add(newKeyword);
                await unitOfWork.Save();
            }
            else
            {
                newKeyword.Id = keywordId;
            }



            unitOfWork.Keywords_In_KeywordGroup.Add(new Keyword_In_KeywordGroup
            {
                KeywordGroupId = item.Id,
                KeywordId = newKeyword.Id
            });


            IEnumerable<int> productIds = await unitOfWork.KeywordGroups_Belonging_To_Product.GetCollection(x => x.KeywordGroupId == item.Id, x => x.ProductId);

            if (productIds.Count() > 0)
            {
                unitOfWork.ProductKeywords.AddRange(productIds.Select(x => new ProductKeyword
                {
                    ProductId = x,
                    KeywordId = newKeyword.Id
                }));
            }


            await unitOfWork.Save();

            return Ok(newKeyword.Id);
        }







        [HttpPut]
        public async Task<ActionResult> UpdateKeyword(ItemViewModel updatedProperty)
        {
            string keywordName = updatedProperty.Name.Trim().ToLower();

            if (await unitOfWork.Keywords.Any(x => x.Name == keywordName)) return Ok();


            Keyword keyword = await unitOfWork.Keywords.Get(updatedProperty.Id);

            keyword.Name = keywordName;

            // Update and save
            unitOfWork.Keywords.Update(keyword);
            await unitOfWork.Save();

            return Ok();
        }




        [HttpDelete]
        public async Task<ActionResult> DeleteKeyword(int id)
        {
            Keyword keyword = await unitOfWork.Keywords.Get(id);

            unitOfWork.Keywords.Remove(keyword);
            await unitOfWork.Save();

            return Ok();
        }





        [HttpDelete]
        [Route("Groups")]
        public async Task<ActionResult> DeleteKeywordGroup(int id)
        {
            KeywordGroup keywordGroup = await unitOfWork.KeywordGroups.Get(id);

            IEnumerable<Keyword> keywords = await unitOfWork.Keywords_In_KeywordGroup.GetCollection(x => x.KeywordGroupId == id, x => x.Keyword);

            unitOfWork.Keywords.RemoveRange(keywords);

            unitOfWork.KeywordGroups.Remove(keywordGroup);
            await unitOfWork.Save();

            return Ok();
        }




        [HttpDelete]
        [Route("Remove")]
        public async Task<ActionResult> RemoveKeyword([FromQuery] int[] ids)
        {
            Keyword_In_KeywordGroup keyword_In_KeywordGroup = await unitOfWork.Keywords_In_KeywordGroup.Get(x => x.KeywordGroupId == ids[0] && x.KeywordId == ids[1]);

            unitOfWork.Keywords_In_KeywordGroup.Remove(keyword_In_KeywordGroup);
            await unitOfWork.Save();

            return Ok();
        }
    }
}
