﻿using System.Runtime.Serialization;
using TetriNET.Common.Attributes;
using TetriNET.Common.Helpers;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Specials // Specials index start after last piece
    {
        [EnumMember]
        [Special(false)]
        Invalid = 0,

        // TetriNET (http://en.wikipedia.org/wiki/TetriNET)
        [EnumMember]
        [Special(true, 'A', "Add Line")]
        AddLines = Pieces.MaxPieces + 1,
        [EnumMember]
        [Special(true, 'C', "Clear Line")]
        ClearLines,
        [EnumMember]
        [Special(true, 'N', "Nuke Field")]
        NukeField,
        [EnumMember]
        [Special(true, 'R', "Random Blocks Clear")]
        RandomBlocksClear,
        [EnumMember]
        [Special(true, 'S', "Switch Fields")]
        SwitchFields,
        [EnumMember]
        [Special(true, 'B', "Clear Special Blocks")]
        ClearSpecialBlocks,
        [EnumMember]
        [Special(true, 'G', "Block Gravity")]
        BlockGravity,
        [EnumMember]
        [Special(true, 'Q', "Block Quake")]
        BlockQuake,
        [EnumMember]
        [Special(true, 'O', "Block Bomb")]
        BlockBomb,

        // TetriNET 2 (http://harddrop.com/wiki/Tetrinet2 or http://en.wikipedia.org/wiki/TetriNET or http://web.archive.org/web/20070623140748/www.tetrinet2.com/?page=overview_specials)
        [EnumMember]
        [Special(true, 'V', "Clear Column")]
        ClearColumn,
        [EnumMember]
        [Special(false)]
        Immunity, // TODO: implement
        [EnumMember]
        [Special(true, 'D', "Darkness")]
        Darkness,
        [EnumMember]
        [Special(true, 'F', "Confusion")]
        Confusion,
        [Special(false)]
        Mutation, // TODO: implement

        // Blocktrix (http://en.wikipedia.org/wiki/TetriNET)
        [EnumMember]
        [Special(false)]
        ZebraField, // will be available when Left Gravity is implemented
        // NOT IMPLEMENTED Piece Change
        [EnumMember]
        [Special(false)]
        LeftGravity, // TODO: implement

        // BlockBattle (http://blockbattle.net/tutorial)
        // NOT IMPLEMENTED Multiplier
        // NOT IMPLEMENTED Pause
    }
}
