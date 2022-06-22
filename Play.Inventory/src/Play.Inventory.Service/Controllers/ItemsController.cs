using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controller
{
    [ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        private readonly IRepository<InventoryItem> itemRepos;

        public ItemController(IRepository<InventoryItem> itemRepository)
        {
            this.itemRepos = itemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var items = (await itemRepos.GetAllAsync(item => item.UserId == userId))
                        .Select(item => item.AsDto());
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await itemRepos.GetAsync(
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

                await itemRepos.CreateAsync(inventoryItem);

            }
            else
            {
                inventoryItem.Quantity += inventoryItem.Quantity;
                await itemRepos.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}