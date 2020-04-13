using Soneta.Business;
using Soneta.Business.App;
using Soneta.Repozytorium;
using Soneta.Types;
using System;
using System.ComponentModel;
using System.Text;

/*
 * Do poprawnego działania dodatku należy podpiąć następujące dllki:
 * Soneta.Repozytorium.dll, Soneta.Repozytorium.UI.dll, Octokit.dll
 */
[assembly: ModuleType("Repozytorium", typeof(RepozytoriumModule), 4, "SonetaRepozytorium", 1, VersionNumber = 1)]

namespace Soneta.Repozytorium
{
    public class RepozytoriumModule : Module
    {
        public static RepozytoriumModule GetInstance(ISessionable session) => (RepozytoriumModule)session?.Session?.Modules[moduleInfo];

        private static ModuleInfo moduleInfo = new ModuleInfo(session => new RepozytoriumModule(session));

        RepozytoriumModule(Session session) : base(session) { }

        private static TableInfo tableInfoRepozytoria = new TableInfo.Create<Repozytoria, Repozytorium, RepozytoriumRecord>("Repozytorium");

        public Repozytoria Repozytoria => (Repozytoria)Session.Tables[tableInfoRepozytoria];

        private static KeyInfo keyInfoRepozytoriumWgAdresu = new KeyInfo(tableInfoRepozytoria, table => new RepozytoriumTable.WgAdresKey(table))
        {
            Name = "WgAdres",
            Unique = true,
            PrimaryKey = true,
            KeyFields = new[] { "Adres" },
        };

        [Caption("Tabela z repozytoriami")]
        public abstract class RepozytoriumTable : GuidedTable
        {
            protected RepozytoriumTable() { }

            public class WgAdresKey : Key<Repozytorium>
            {
                internal WgAdresKey(Table table) : base(table)
                {
                }

                protected override object[] GetData(Row row, Record rec) => new object[] {
                    ((RepozytoriumRecord)rec).Adres.TrimEnd()
                };

                public Repozytorium this[string adres] => (Repozytorium)Find(adres);
            }

            public WgAdresKey WgAdres => (WgAdresKey)Session.Keys[keyInfoRepozytoriumWgAdresu];

            public new RepozytoriumModule Module => (RepozytoriumModule)base.Module;
            
            public new Repozytorium this[int id] => (Repozytorium)base[id];
            
            public new Repozytorium[] this[int[] ids] => (Repozytorium[])base[ids];

            protected override Row CreateRow(RowCreator creator) => new Repozytorium();

            [Langs.TranslateIgnore]
            protected override sealed void PrepareNames(StringBuilder names, string divider)
            {
                names.Append(divider); names.Append("Guid");
                names.Append(divider); names.Append("Adres");
                names.Append(divider); names.Append("Typ");
                names.Append(divider); names.Append("Opis");
            }
        }

        public abstract class RepozytoriumRow : GuidedRow
        {
            private RepozytoriumRecord record;

            protected override void AssignRecord(Record rec)
            {
                record = (RepozytoriumRecord)rec;
            }

            protected RepozytoriumRow() : base(true)
            {
            }

            [MaxLength(200)]
            [Required]
            public string Adres
            {
                get
                {
                    if (record == null) GetRecord();
                    return record.Adres;
                }
                set
                {
                    RepozytoriumSchema.AdresBeforeEdit?.Invoke((Repozytorium)this, ref value);
                    if (value != null) value = value.TrimEnd();
                    if (string.IsNullOrEmpty(value)) throw new RequiredException(this, "Adres");
                    if (value.Length > AdresLength) throw new ValueToLongException(this, "Adres", AdresLength);
                    GetEdit(record == null, false);
                    record.Adres = value;
                    if (State != RowState.Detached)
                    {
                        ResyncSet(keyInfoRepozytoriumWgAdresu);
                    }
                    RepozytoriumSchema.AdresAfterEdit?.Invoke((Repozytorium)this);
                }
            }

            public const int AdresLength = 200;

            [Required]
            public RepositoryType Typ
            {
                get
                {
                    if (record == null) GetRecord();
                    return record.Typ;
                }
                set
                {
                    RepozytoriumSchema.TypBeforeEdit?.Invoke((Repozytorium)this, ref value);
                    if (!Enum.IsDefined(typeof(RepositoryType), value)) throw new RequiredException(this, "Typ");
                    GetEdit(record == null, false);
                    record.Typ = value;
                    RepozytoriumSchema.TypAfterEdit?.Invoke((Repozytorium)this);
                }
            }

            [MaxLength(200)]
            public string Opis
            {
                get
                {
                    if (record == null) GetRecord();
                    return record.Opis;
                }
                set
                {
                    RepozytoriumSchema.OpisBeforeEdit?.Invoke((Repozytorium)this, ref value);
                    if (value != null) value = value.TrimEnd();
                    if (value.Length > OpisLength) throw new ValueToLongException(this, "Opis", OpisLength);
                    GetEdit(record == null, false);
                    record.Opis = value;
                    RepozytoriumSchema.OpisAfterEdit?.Invoke((Repozytorium)this);
                }
            }

            public const int OpisLength = 200;
            
            [Browsable(false)]
            public new Repozytoria Table => (Repozytoria)base.Table;

            [Browsable(false)]
            public RepozytoriumModule Module => Table.Module;

            protected override TableInfo TableInfo => tableInfoRepozytoria;

            public sealed override AccessRights GetObjectRight()
            {
                AccessRights ar = CalcObjectRight();
                RepozytoriumSchema.OnCalcObjectRight?.Invoke((Repozytorium)this, ref ar);
                return ar;
            }

            protected sealed override AccessRights GetParentsObjectRight()
            {
                AccessRights result = CalcParentsObjectRight();
                RepozytoriumSchema.OnCalcParentsObjectRight?.Invoke((Repozytorium)this, ref result);
                return result;
            }

            protected override bool CalcReadOnly()
            {
                bool result = false;
                RepozytoriumSchema.OnCalcReadOnly?.Invoke((Repozytorium)this, ref result);
                return result;
            }

            class AdresRequiredVerifier : RequiredVerifier
            {
                internal AdresRequiredVerifier(IRow row) : base(row, "Adres")
                {
                }
                protected override bool IsValid() => !(string.IsNullOrEmpty(((RepozytoriumRow)Row).Adres));
            }

            class TypRequiredVerifier : RequiredVerifier
            {
                internal TypRequiredVerifier(IRow row) : base(row, "Typ")
                {
                }
                protected override bool IsValid() => Enum.IsDefined(typeof(RepositoryType), ((RepozytoriumRow)Row).Typ);
            }

            protected override void OnAdded()
            {
                base.OnAdded();
                Session.Verifiers.Add(new AdresRequiredVerifier(this));
                Session.Verifiers.Add(new TypRequiredVerifier(this));
                RepozytoriumSchema.OnAdded?.Invoke((Repozytorium)this);
            }

            protected override void OnLoaded()
            {
                base.OnLoaded();
                RepozytoriumSchema.OnLoaded?.Invoke((Repozytorium)this);
            }

            protected override void OnEditing()
            {
                base.OnEditing();
                RepozytoriumSchema.OnEditing?.Invoke((Repozytorium)this);
            }

            protected override void OnDeleting()
            {
                base.OnDeleting();
                RepozytoriumSchema.OnDeleting?.Invoke((Repozytorium)this);
            }

            protected override void OnDeleted()
            {
                base.OnDeleted();
                RepozytoriumSchema.OnDeleted?.Invoke((Repozytorium)this);
            }

            protected override void OnRepacked()
            {
                base.OnRepacked();
                RepozytoriumSchema.OnRepacked?.Invoke((Repozytorium)this);
            }

        }

        public sealed class RepozytoriumRecord : GuidedRecord
        {
            [Required]
            [MaxLength(200)]
            public string Adres = "";
            [Required]
            public RepositoryType Typ;
            [MaxLength(200)]
            public string Opis = "";

            public override Record Clone()
            {
                RepozytoriumRecord rec = (RepozytoriumRecord)MemberwiseClone();
                return rec;
            }

            public override void Load(RecordReader creator)
            {
                Guid = creator.Read_guid();
                Adres = creator.Read_string();
                Typ = (RepositoryType)creator.Read_int();
                Opis = creator.Read_string();
            }
        }

        public static class RepozytoriumSchema
        {
            internal static RowDelegate<RepozytoriumRow, string> AdresBeforeEdit;
            public static void AddAdresBeforeEdit(RowDelegate<RepozytoriumRow, string> value)
                => AdresBeforeEdit = (RowDelegate<RepozytoriumRow, string>)Delegate.Combine(AdresBeforeEdit, value);

            internal static RowDelegate<RepozytoriumRow> AdresAfterEdit;
            public static void AddAdresAfterEdit(RowDelegate<RepozytoriumRow> value)
                => AdresAfterEdit = (RowDelegate<RepozytoriumRow>)Delegate.Combine(AdresAfterEdit, value);

            internal static RowDelegate<RepozytoriumRow, RepositoryType> TypBeforeEdit;
            public static void AddTypBeforeEdit(RowDelegate<RepozytoriumRow, RepositoryType> value)
                => TypBeforeEdit = (RowDelegate<RepozytoriumRow, RepositoryType>)Delegate.Combine(TypBeforeEdit, value);

            internal static RowDelegate<RepozytoriumRow> TypAfterEdit;
            public static void AddTypAfterEdit(RowDelegate<RepozytoriumRow> value)
                => TypAfterEdit = (RowDelegate<RepozytoriumRow>)Delegate.Combine(TypAfterEdit, value);

            internal static RowDelegate<RepozytoriumRow, string> OpisBeforeEdit;
            public static void AddOpisBeforeEdit(RowDelegate<RepozytoriumRow, string> value)
                => OpisBeforeEdit = (RowDelegate<RepozytoriumRow, string>)Delegate.Combine(OpisBeforeEdit, value);

            internal static RowDelegate<RepozytoriumRow> OpisAfterEdit;
            public static void AddOpisAfterEdit(RowDelegate<RepozytoriumRow> value)
                => OpisAfterEdit = (RowDelegate<RepozytoriumRow>)Delegate.Combine(OpisAfterEdit, value);

            internal static RowDelegate<RepozytoriumRow> OnLoaded;
            public static void AddOnLoaded(RowDelegate<RepozytoriumRow> value)
                => OnLoaded = (RowDelegate<RepozytoriumRow>)Delegate.Combine(OnLoaded, value);

            internal static RowDelegate<RepozytoriumRow> OnAdded;
            public static void AddOnAdded(RowDelegate<RepozytoriumRow> value)
                => OnAdded = (RowDelegate<RepozytoriumRow>)Delegate.Combine(OnAdded, value);

            internal static RowDelegate<RepozytoriumRow> OnEditing;
            public static void AddOnEditing(RowDelegate<RepozytoriumRow> value)
                => OnEditing = (RowDelegate<RepozytoriumRow>)Delegate.Combine(OnEditing, value);

            internal static RowDelegate<RepozytoriumRow> OnDeleting;
            public static void AddOnDeleting(RowDelegate<RepozytoriumRow> value)
                => OnDeleting = (RowDelegate<RepozytoriumRow>)Delegate.Combine(OnDeleting, value);

            internal static RowDelegate<RepozytoriumRow> OnDeleted;
            public static void AddOnDeleted(RowDelegate<RepozytoriumRow> value)
                => OnDeleted = (RowDelegate<RepozytoriumRow>)Delegate.Combine(OnDeleted, value);

            internal static RowDelegate<RepozytoriumRow> OnRepacked;
            public static void AddOnRepacked(RowDelegate<RepozytoriumRow> value)
                => OnRepacked = (RowDelegate<RepozytoriumRow>)Delegate.Combine(OnRepacked, value);

            internal static RowAccessRightsDelegate<RepozytoriumRow> OnCalcObjectRight;
            public static void AddOnCalcObjectRight(RowAccessRightsDelegate<RepozytoriumRow> value)
                => OnCalcObjectRight = (RowAccessRightsDelegate<RepozytoriumRow>)Delegate.Combine(OnCalcObjectRight, value);

            internal static RowAccessRightsDelegate<RepozytoriumRow> OnCalcParentsObjectRight;
            public static void AddOnCalcParentsObjectRight(RowAccessRightsDelegate<RepozytoriumRow> value)
                => OnCalcParentsObjectRight = (RowAccessRightsDelegate<RepozytoriumRow>)Delegate.Combine(OnCalcParentsObjectRight, value);

            internal static RowReadOnlyDelegate<RepozytoriumRow> OnCalcReadOnly;
            public static void AddOnCalcReadOnly(RowReadOnlyDelegate<RepozytoriumRow> value)
                => OnCalcReadOnly = (RowReadOnlyDelegate<RepozytoriumRow>)Delegate.Combine(OnCalcReadOnly, value);
        }
    }
    
    public static class StaticsRepozytoriumModule
    {
        public static RepozytoriumModule GetRepozytorium(this Session session) => RepozytoriumModule.GetInstance(session);
    }
}

