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
        private const string ResultKey = "ResultState"; // R6: a separate label, so the result never overwrites the wizard's answers

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

        // ====================================================================
        // R6: RESULT HELPERS
        // Same pattern as the WizardState helpers above, but for the result
        // that Recommend works out and the Results page shows.
        // ====================================================================

        // Save the result to session (Object -> JSON String)
        public static void SetResultState(this ISession session, ResultState result)
        {
            session.SetString(ResultKey, JsonSerializer.Serialize(result));
        }

        // Load the result from session (JSON String -> Object). Null if there isn't one.
        public static ResultState? GetResultState(this ISession session)
        {
            var json = session.GetString(ResultKey);
            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<ResultState>(json);
        }

        // Delete the result from session, so "Start again" really starts again
        public static void ClearResultState(this ISession session)
        {
            session.Remove(ResultKey);
        }

    }
}
