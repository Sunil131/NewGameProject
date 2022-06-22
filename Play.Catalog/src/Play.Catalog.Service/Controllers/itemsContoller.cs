using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    //https://localhost:5001/items
    [ApiController]
    [Route("items")]
    public class ItemsContoller : ControllerBase
    {
        ////This is a temporary test list.
        // public static readonly List<ItemDto> items = new()
        // {
        //     new ItemDto(Guid.NewGuid(), "Potion", "Restore a small amount of HP", 5, DateTimeOffset.UtcNow),
        //     new ItemDto(Guid.NewGuid(), "AntiDote", "Cures Poison", 7, DateTimeOffset.UtcNow),
        //     new ItemDto(Guid.NewGuid(), "Bronze Sword", "Deal a small amount of Damage", 20, DateTimeOffset.UtcNow)
        // };

        private readonly IRepository<Item> itemRepo;

        public ItemsContoller(IRepository<Item> itemRepository)
        {
            this.itemRepo = itemRepository;
        }
        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetAsync()
        {
            var items = (await itemRepo.GetAllAsync()).Select(item => item.AsDto());
            return items;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemRepo.GetAsync(id);
            if (item == null)
            {

                return NotFound();
            }
            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var newItem = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemRepo.CreateAsync(newItem);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = newItem.Id }, newItem);

        }


        [HttpPut("id")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemRepo.GetAsync(id);
            if (existingItem == null)
            {

                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemRepo.UpdateAsync(existingItem);


            ////OLD CODE
            // var existingItem = items.Where(item => item.Id == id).SingleOrDefault();
            // if (existingItem == null)
            // {

            //     return NotFound();
            // }
            // var updateItem = existingItem with
            // {
            //     Name = updateItemDto.Name,
            //     Description = updateItemDto.Description,
            //     Price = updateItemDto.Price
            // };

            // var index = items.FindIndex(existingItem => existingItem.Id == id);
            // items[index] = updateItem;

            return NoContent();


        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            ////OLD CODE
            // var index = items.FindIndex(existingItem => existingItem.Id == id);
            // if (index == 0)
            // {

            //     return NotFound();
            // }
            // items.RemoveAt(index);

            var existingItem = await itemRepo.GetAsync(id);
            if (existingItem == null)
            {

                return NotFound();
            }

            await itemRepo.RemoveAsync(existingItem.Id);
            return NoContent();

        }

    }
}