using Soneta.Business.UI;

[assembly: FolderView("Repozytorium",
    Priority = 0,
    Description = "Podgląd podpiętych repozytoriów",
    BrickColor = FolderViewAttribute.BlueBrick,
    TableName = "Repozytoria",
    ViewType = typeof(Soneta.Repozytorium.UI.RepositoriesViewInfo),
    Icon = "TableFolder.ico"
)]