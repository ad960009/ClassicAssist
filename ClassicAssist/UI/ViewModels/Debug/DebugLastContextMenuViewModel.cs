// Copyright (C) $CURRENT_YEAR$ Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugLastContextMenuViewModel : BaseViewModel
    {
        private IEnumerable<ContextMenuEntry> _items;

        internal IEnumerable<ContextMenuEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public DebugLastContextMenuViewModel()
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0xBF,
                new[]
                {
                    PacketFilterConditions.ShortAtPositionCondition( 0x14, 3 ),
                }, ( bytes, info ) =>
                {
                    IEnumerable<ContextMenuEntry> entries = ParseContextMenuEntries( bytes );
                    Items = entries;
                } );
            Engine.AddReceiveFilter( pfi );
        }

        private static IEnumerable<ContextMenuEntry> ParseContextMenuEntries( byte[] packet )
        {
            PacketReader reader = new PacketReader( packet, packet.Length, false );

            reader.ReadInt16();
            int type = reader.ReadInt16();
            int serial = reader.ReadInt32();
            int len = reader.ReadByte();

            int entry, cliloc, flags, hue;

            List<ContextMenuEntry> entries = new List<ContextMenuEntry>();

            switch ( type )
            {
                case 1: // Old Type
                    for ( int x = 0; x < len; x++ )
                    {
                        entry = reader.ReadInt16();
                        cliloc = reader.ReadInt16() + 3000000;
                        flags = reader.ReadInt16();
                        hue = 0;

                        if ( ( flags & 0x20 ) == 0x20 )
                        {
                            hue = reader.ReadInt16();
                        }

                        string text = Cliloc.GetProperty( cliloc );
                        entries.Add( new ContextMenuEntry
                        {
                            Index = entry,
                            Cliloc = cliloc,
                            Flags = (ContextMenuFlags) flags,
                            Hue = hue,
                            Text = text
                        } );
                    }

                    break;
                case 2: // KR -> SA3D -> 2D post 7.0.0.0
                    for ( int x = 0; x < len; x++ )
                    {
                        cliloc = reader.ReadInt32();
                        entry = reader.ReadInt16();
                        flags = reader.ReadInt16();
                        hue = 0;

                        if ( ( flags & 0x20 ) == 0x20 )
                        {
                            hue = reader.ReadInt16();
                        }

                        string text = Cliloc.GetProperty( cliloc );

                        entries.Add( new ContextMenuEntry
                        {
                            Index = entry,
                            Cliloc = cliloc,
                            Flags = (ContextMenuFlags) flags,
                            Hue = hue,
                            Text = text
                        } );
                    }

                    break;
            }

            return entries;
        }

        public class ContextMenuEntry
        {
            public int Cliloc { get; set; }
            public ContextMenuFlags Flags { get; set; }
            public int Hue { get; set; }
            public int Index { get; set; }
            public string Text { get; set; }
        }

        [Flags]
        public enum ContextMenuFlags
        {
            Enabled = 0x00,
            Disabled = 0x01,
            Highlighted = 0x04,
            Coloured = 0x20
        }
    }
}
