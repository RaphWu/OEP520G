using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Constants
{
    internal class DB
    {
        internal const string TABLE_NAME_DISPENSER = "Dispenser";
        internal const string CREATE_TABLE_DISPENSER = @"CREATE TABLE IF NOT EXISTS [Dispenser](
[Id] INTEGER PRIMARY KEY NOT NULL UNIQUE,
[DatumX] DOUBLE,
[DatumY] DOUBLE,
[DatumZ] DOUBLE,
[UvPosition] INTEGER,
[UvTime] DOUBLE,
[UvPreWait] DOUBLE,
[UvPostWait] DOUBLE) WITHOUT ROWID;";

        internal const string TABLE_NAME_DISPENSE_SHAPE = "DispensingShape";
        internal const string CREATE_TABLE_DISPENSE_SHAPE = @"CREATE TABLE IF NOT EXISTS [DispensingShape](
[Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
[Title] TEXT NOT NULL);";

        internal const string TABLE_NAME_DISPENSE_SEQUENCE = "DispensingSequence";
        internal const string CREATE_TABLE_DISPENSE_SEQUENCE = @"CREATE TABLE IF NOT EXISTS [DispensingSequence](
[SeqNo] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[ShapeId] INTEGER NOT NULL,
[Type] INTEGER,
[GroupNo] INTEGER,
[OffsetX] DOUBLE,
[OffsetY] DOUBLE,
[OffsetZ] DOUBLE,
[OffsetR] DOUBLE) WITHOUT ROWID;";

        internal const string TABLE_NAME_DISPENSE_GROUP = "DispensingGroup";
        internal const string CREATE_TABLE_DISPENSE_GROUP = @"CREATE TABLE IF NOT EXISTS [DispensingGroup](
[GroupNo] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[ShapeId] INTEGER,
[DspSpeed] DOUBLE DEFAULT (100.0),
[SpeedR] DOUBLE DEFAULT (150.0),
[SWait] INTEGER,
[EShot] INTEGER,
[PreStop] DOUBLE,
[EWait] INTEGER,
[UpXY] DOUBLE,
[UpZ] DOUBLE,
[UpSpeed] DOUBLE,
[UpDelay] INTEGER,
[UpWay] INTEGER) WITHOUT ROWID;";
    }
}
