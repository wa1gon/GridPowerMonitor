using GridPowerMonitor.Model;

namespace GridPowerMonitor;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;


[ApiController]
[Route("api/[controller]")]
public class HeartBeatMonitorController : ControllerBase
{
    private static readonly  List<HeartBeats> Items = [];
    
    
    [HttpGet("{id}")]
    public ActionResult<string> GetItem(string id)
    {
        var existingItem = Items.FirstOrDefault(hb => hb.DeviceId == id);
        if (existingItem != null)
        {
            existingItem.LastHeartBeat = DateTime.Now;
        }
        else
        {
            var hb = new HeartBeats() { DeviceId = id, LastHeartBeat = DateTime.Now };
            Items.Add(hb);
        }
        return Ok($"foobar: {id}");
    }
    [HttpGet("lastupdate/{id}")]
    public ActionResult<string> GetLastUpdate(string id)
    {
        var existingItem = Items.FirstOrDefault(hb => hb.DeviceId == id);
        if (existingItem != null)
        {
            return Ok(existingItem.LastHeartBeat.ToString("o")); // Return the last heartbeat in ISO 8601 format
        }
        else
        {
            return NotFound($"Item with DeviceId {id} not found.");
        }
    }
}
