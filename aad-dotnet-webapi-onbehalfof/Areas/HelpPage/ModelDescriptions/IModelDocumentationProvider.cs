using System;
using System.Reflection;

namespace aad_dotnet_webapi_onbehalfof.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}