using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microservices.Infrastructure;
using Microservices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microservices.Controllers
{
    [Route("api/[controller]")] //route prefix
    [ApiController]
    public class EventsController : ControllerBase
    {
        private EventDbContext db;
        public EventsController(EventDbContext dbContext)
        {
            db = dbContext;
        }

        //GET /api/events
        [HttpGet(Name = "GetAll")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<EventInfo>> GetEvents()
        {
            //throw new NullReferenceException("Something went wrong");
            var events = db.Events.ToList();
            return Ok(events); //returns with status code 200
        }

        //POST /api/events
        [Authorize]
        [HttpPost(Name = "AddEvent")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<EventInfo>> AddEventAsync([FromBody]EventInfo eventInfo)
        {
            if (ModelState.IsValid)
            {
                var result = await db.Events.AddAsync(eventInfo);
                await db.SaveChangesAsync();
                //return CreatedAtRoute("GetById", new { id = result.Entity.Id }, result.Entity);
                return CreatedAtAction(nameof(GetEventAsync), new { id = result.Entity.Id }, result.Entity);
                //return Created("", result.Entity); //return the status code 201
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        ////POST /api/events
        //[HttpPost]
        //public ActionResult<EventInfo> AddEvent([FromBody]EventInfo eventInfo)
        //{
        //    var result = db.Events.Add(eventInfo);
        //    db.SaveChanges();
        //    return CreatedAtRoute("GetById", new { id = result.Entity.Id }, result.Entity);
        //    //return CreatedAtAction(nameof(GetEvent), new { id = result.Entity.Id }, result.Entity);
        //    //return Created("", result.Entity); //return the status code 201
        //}

        //GET /api/events/{id}
        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EventInfo>> GetEventAsync([FromRoute]int id)
        {
            var eventInfo = await db.Events.FindAsync(id);

            if (eventInfo != null)
            {
                return Ok(eventInfo);
            }
            else
            {
                return NotFound("Item you are searching not found");
            }
        }
    }
}