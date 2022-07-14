using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Client;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controller
{
    [ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        private readonly IRepository<InventoryItem> inventoryItemRepos;
        //private readonly CatalogClient catalogClient;
        private readonly IRepository<CatalogItem> catalogItemRepo;


        public ItemController(IRepository<InventoryItem> inventoryItemRepos, IRepository<CatalogItem> catalogItemRepo)
        {
            this.inventoryItemRepos = inventoryItemRepos;
            this.catalogItemRepo = catalogItemRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            // var items = (await itemRepos.GetAllAsync(item => item.UserId == userId))
            //             .Select(item => item.AsDto());
            //var catalogItems = await catalogClient.GetCatalogItemsAsync();

            var inventoryItemEntities = await inventoryItemRepos.GetAllAsync(item => item.UserId == userId);
            var itemsIds = inventoryItemEntities.Select(item => item.CatalogItemId);
            var catalogItemEntities = await catalogItemRepo.GetAllAsync(item => itemsIds.Contains(item.Id));


            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await inventoryItemRepos.GetAsync(
                item => item.UserId == grantItemsDto.userId && item.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.userId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await inventoryItemRepos.CreateAsync(inventoryItem);

            }
            else
            {
                inventoryItem.Quantity += inventoryItem.Quantity;
                await inventoryItemRepos.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}