using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace DaOAuthCore.WebServer.Models.Binders
{
    public class ChangeAuthorizationClientModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = new ChangeAuthorizationClientModel()
            {
                ClientId = bindingContext.ValueProvider.GetValue("client_id").FirstValue
            };

            bool autorize = false;
            if(bool.TryParse(bindingContext.ValueProvider.GetValue("authorize").FirstValue, out autorize))
            {
                result.Authorize = true;
            }            

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}
