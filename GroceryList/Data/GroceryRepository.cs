using GroceryList.Models;
using GroceryList.Models.Forms;
using GroceryList.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public class GroceryRepository
    {
        private readonly DataFileService fileService;
        private readonly IHttpContextAccessor context;
        public GroceryRepository(DataFileService dataFileService, IHttpContextAccessor contextAccessor)
        {
            fileService = dataFileService;
            context = contextAccessor;
        }

        public List<GroceryItem> GetGroceries()
        {
            // get Home ID & correct current data file

        }

        public GroceryItem Add(GroceryItemForm model)
        {
        }
    }
}
