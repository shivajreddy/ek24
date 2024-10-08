﻿using Autodesk.Revit.DB;
using System.Windows;
using ek24.UI.ViewModels.ChangeBrand;


namespace ek24.UI.Views.ChangeBrand;


public partial class ChangeBrandView : Window
{

    public ChangeBrandView(Document doc)
    {
        InitializeComponent();

        DataContext = new ChangeBrandViewModel(doc);
    }

    public string ChosenBrandName => (DataContext as ChangeBrandViewModel).ChosenBrand.BrandName;

}
