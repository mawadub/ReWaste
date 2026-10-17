using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RE_Waste.Models;

namespace RE_Waste.Extensions
{
    // ====================================================================
    // SESSION HELPERS
    // Browsers only store text, but our WizardState is an object. 
    // We use System.Text.Json to serialize the object into a string for
    // storage, and deserialize it back into an object when we need it.
    // ====================================================================
    public static class SessionExtensions
    {
        private const string Key = "WizardState"; // The label used to find our data in the session

        // Save the object to session (Object -> JSON String)
        public static void SetWizardState(this ISession session, WizardState state)
        {
            session.SetString(Key, JsonSerializer.Serialize(state));
        }

        // Load the object from session (JSON String -> Object)
        public static WizardState? GetWizardState(this ISession session)
        {
            var json = session.GetString(Key);
            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<WizardState>(json);
        }

        // Delete the object from session completely
        public static void ClearWizardState(this ISession session)
        {
            session.Remove(Key);
        }

    }
}
