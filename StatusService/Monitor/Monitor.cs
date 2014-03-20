﻿using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StatusService
{
    class Monitor : BaseMonitor
    {
        DateTime nextNotify = DateTime.MaxValue;


        public Monitor( IPEndPoint server )
            : base( server )
        {
        }


        protected override void OnConnected( SteamClient.ConnectedCallback callback )
        {
            if ( callback.Result != EResult.OK )
            {
                SteamMonitor.Instance.NotifyCMOffline( this, callback.Result );
                return;
            }

            base.OnConnected( callback );
        }

        protected override void OnDisconnected( SteamClient.DisconnectedCallback callback )
        {
            base.OnDisconnected( callback );

            if ( !IsDisconnecting )
            {
                // if we're not forcibly disconnecting this instance, notify that the CM is offline
                SteamMonitor.Instance.NotifyCMOffline( this );
            }
        }

        protected override void OnLoggedOn( SteamUser.LoggedOnCallback callback )
        {
            base.OnLoggedOn( callback );

            if ( callback.Result != EResult.OK )
            {
                SteamMonitor.Instance.NotifyCMOffline( this, callback.Result );
                return;
            }

            SteamMonitor.Instance.NotifyCMOnline( this );

            nextNotify = DateTime.Now + TimeSpan.FromMinutes( 1 );
        }

        protected override void OnLoggedOff( SteamUser.LoggedOffCallback callback )
        {
            base.OnLoggedOff( callback );

            SteamMonitor.Instance.NotifyCMOffline( this, callback.Result );
        }


        protected override void Tick()
        {
            // run callbacks
            base.Tick();

            if ( DateTime.Now >= nextNotify )
            {
                if ( Client.IsConnected )
                {
                    SteamMonitor.Instance.NotifyCMOnline( this );
                }
                else
                {
                    SteamMonitor.Instance.NotifyCMOffline( this );
                }

                nextNotify = DateTime.Now + TimeSpan.FromMinutes( 1 );
            }
        }
    }
}
