using System;
using System.Threading;
using Steamworks;

namespace SteamWorkshopUploader
{
    public static class Uploader
    {
        private const int RIMWORLD_APP_INT = 294100;
        private static AppId_t RIMWORLD = new AppId_t( RIMWORLD_APP_INT );
        private static AutoResetEvent ready = new AutoResetEvent(false);
        private static CallResult<SubmitItemUpdateResult_t> submitResultCallback;
        private static CallResult<CreateItemResult_t> createResultCallback;
        private static bool _initialized;

        public static void Init()
        {
            Environment.SetEnvironmentVariable("SteamAppId", RIMWORLD_APP_INT.ToString());
            try
            {
                _initialized = SteamAPI.Init();
                if ( !_initialized )
                {
                    Console.WriteLine( "Steam API failed to initialize." );
                }
                else
                {
                    SteamClient.SetWarningMessageHook( ( severity, text ) => Console.WriteLine( text.ToString() ) );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: ");
                Console.Write(e.Message);
            }
        }

        public static bool Upload( Mod mod, string changenote )
        {
            bool creating = false;
            if ( mod.PublishedFileId == PublishedFileId_t.Invalid )
            {
                // create item first.
                creating = true;
                Console.WriteLine( "no PublishedFileId found, creating new mod..." );

                if ( !Create( mod ) )
                {
                    throw new Exception( "mod creation failed!" );
                }
            }

            // create timestamp to force steam to accept the update as a change
            mod.TimeStamp();

            // set up steam API call
            var handle = SteamUGC.StartItemUpdate( RIMWORLD, mod.PublishedFileId );
            SetItemAttributes( handle, mod, creating );

            // start async call
            var call = SteamUGC.SubmitItemUpdate( handle, changenote );
            submitResultCallback = CallResult<SubmitItemUpdateResult_t>.Create( OnItemSubmitted );
            submitResultCallback.Set( call );

            // keep checking for async call to complete
            var loading = new LoadingIndicator();
            while ( !ready.WaitOne( 50 ) )
            {
                var status = SteamUGC.GetItemUpdateProgress( handle, out ulong done, out ulong total );
                SteamAPI.RunCallbacks();
                ClearLine();
                if ( status != EItemUpdateStatus.k_EItemUpdateStatusInvalid )
                {
                    if ( total > 0 )
                        Console.Write( status + ": " + ( done / total ).ToString( "P" ) + " completed. " + loading );
                    else
                        Console.Write( status + "... " + loading );
                }
            }

            // we have completed!
            if ( submitResult.m_eResult != EResult.k_EResultOK )
                Console.WriteLine( "\n" + submitResult.m_eResult );
            else
                Console.WriteLine( "" );
            return submitResult.m_eResult == EResult.k_EResultOK;
        }

        private static SubmitItemUpdateResult_t submitResult;
        private static void OnItemSubmitted( SubmitItemUpdateResult_t result, bool failure )
        {
            // store result and let the main thread continue
            submitResult = result;
            ready.Set();
        }

        public static bool Create( Mod mod )
        {
            // start async call
            var call = SteamUGC.CreateItem( RIMWORLD, 0 );
            createResultCallback = CallResult<CreateItemResult_t>.Create( OnItemCreated );
            createResultCallback.Set(call);

            // keep checking for async call to complete
            var loading = new LoadingIndicator();
            while (!ready.WaitOne(50))
            {
                SteamAPI.RunCallbacks();
                ClearLine();
                Console.Write( "Waiting for item creation to complete... " + loading );
            }

            // we have completed!
            if ( createResult.m_eResult != EResult.k_EResultOK )
            {
                Console.WriteLine( createResult.m_eResult );
            }
            else
            {
                mod.PublishedFileId = createResult.m_nPublishedFileId;
                Console.WriteLine( "\nNew mod created (" + mod.PublishedFileId + ")" );
            }
            
            return createResult.m_eResult == EResult.k_EResultOK;
        }

        private static CreateItemResult_t createResult;
        private static void OnItemCreated( CreateItemResult_t result, bool failure )
        {
            // store result and let the main thread continue
            createResult = result;
            ready.Set();
        }

        private static void SetItemAttributes( UGCUpdateHandle_t handle, Mod mod, bool creating )
        {
            SteamUGC.SetItemTitle( handle, mod.Name );
            SteamUGC.SetItemTags( handle, mod.Tags );
            SteamUGC.SetItemContent( handle, mod.ContentFolder );

            if (!mod.OriginalUploader)
                return;

            //only the original uploader (i.e. not contributors) can do the following operations
            SteamUGC.SetItemDescription(handle, mod.Description);
            if (mod.Preview != null)
                SteamUGC.SetItemPreview(handle, mod.Preview);
            if (creating)
                SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
        }

        public static void Shutdown()
        {
            SteamAPI.Shutdown();
            _initialized = false;
        }

        private static void ClearLine()
        {
            int line = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line);
        }
    }
}
