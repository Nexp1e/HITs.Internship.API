using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HITs.Internship.API.Swagger
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(AllowAnonymousAttribute)
                                                                               || x.AttributeType == typeof(AuthorizeAttribute));
            var curContrType = context.MethodInfo.DeclaringType;
            while (curContrType != null && attr is null)
            {
                attr = curContrType.CustomAttributes.FirstOrDefault(x =>
                    x.AttributeType == typeof(AllowAnonymousAttribute) ||
                    x.AttributeType == typeof(AuthorizeAttribute));
                curContrType = curContrType.BaseType;
            }

            if (attr?.AttributeType == typeof(AuthorizeAttribute))
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type=ReferenceType.SecurityScheme,
                                    Id="Bearer"
                                }
                            },
                            new string[]{}
                        }
                    }
                };

                if (!operation.Responses.ContainsKey("401"))
                {
                    operation.Responses.Add("401", new OpenApiResponse()
                    {
                        Description = "Unauthorized",
                    });
                }

                if (!operation.Responses.ContainsKey("403"))
                {
                    operation.Responses.Add("403", new OpenApiResponse()
                    {
                        Description = "Forbidden",
                    });
                }
            }
        }
    }

}
