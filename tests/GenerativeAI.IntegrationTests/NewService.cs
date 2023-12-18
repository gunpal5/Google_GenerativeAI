using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeAI;

namespace TryAgiOpenAITests
{
    [GenerativeAIFunctions]
    public interface INewService
    {
        [Description("Should run it")]
        Task<string> RunIt();
    }

    public class NewService : INewService
    {
        public Task<string> RunIt()
        {
            //throw new NotImplementedException();
        }
    }
}
