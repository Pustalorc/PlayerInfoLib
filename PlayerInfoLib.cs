using PlayerInfoLibrary.Configuration;
using PlayerInfoLibrary.Database;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;

namespace PlayerInfoLibrary
{
    public sealed class PlayerInfoLib : RocketPlugin<PlayerInfoLibConfig>
    {
        public static PlayerInfoLib Instance;
        public DatabaseManager database;

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"too_many_parameters", "Too many parameters."},
                {"investigate_help", "<player> [page] - Returns info for players matching the search query."},
                {"delint_help", "<InstanceId> - Uses the numerical Instance ID for a server to remove all player data saved for that server."},
                {"rnint_help", "<InstanceName> - Renames this instance in the database."},
                {"invalid_page", "Error: Invalid page number."},
                {"number_of_records_found", "{0} Records found for: {1}, Page: {2} of {3}"},
                {"delint_invalid", "Error: Invalid Instance ID."},
                {"delint_not_found", "Error: Failed to find Instance ID in the database."},
                {
                    "delint_success",
                    "Successfully Removed all data for this Instance ID, if you removed the data for this server, you will need to reload the plugin for it to be operational again."
                },
                {
                    "rnint_success",
                    "Successfully changed the instance name for this server in the Database, Server should be restarted now."
                },
                {"rnint_not_found", "Error: Failed to set the new instance name to the Database."}
            };

        protected override void Load()
        {
            Instance = this;
            database = new DatabaseManager(Configuration.Instance);

            if (database.Initialized)
                Logger.LogWarning(
                    $"PlayerInfoLib plugin has been loaded, Server Instance ID is: {database.InstanceId}");
            else
                Logger.LogError("There was in issue loading the plugin, please check your config.");
        }

        protected override void Unload()
        {
            database = null;
            Instance = null;
        }
    }
}