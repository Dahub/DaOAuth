using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace DaOAuthCore.WebServer.Models.Binders
{
    public class RevokeClientModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = new RevokeClientModel()
            {
                ClientId = bindingContext.ValueProvider.GetValue("client_id").FirstValue
            };

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}
