using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using cohort.ViewModels;

namespace cohort.Mvc
{
    // http://stackoverflow.com/questions/4316301/asp-net-mvc-2-bind-a-models-property-to-a-different-named-value

    public class CohortModelBinder : DefaultModelBinder
    {
        protected override PropertyDescriptorCollection GetModelProperties(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var toReturn = base.GetModelProperties(controllerContext, bindingContext);
            var additional = new List<PropertyDescriptor>();

            var properties = GetTypeDescriptor(controllerContext, bindingContext).GetProperties();
            
            foreach (var p in properties.Cast<PropertyDescriptor>())
            {
                foreach (var attr in p.Attributes.OfType<FormName>())
                {
                    additional.Add(new AliasedPropertyDescriptor(attr.Alias, p));

                    if (bindingContext.PropertyMetadata.ContainsKey(p.Name))
                    {
                        bindingContext.PropertyMetadata.Add(attr.Alias, bindingContext.PropertyMetadata[p.Name]);
                    }
                }
            }
            return new PropertyDescriptorCollection(toReturn.Cast<PropertyDescriptor>().Concat(additional).ToArray());
        }
    }
}