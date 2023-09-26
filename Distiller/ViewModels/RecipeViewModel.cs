using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Distiller.Models;

namespace Distiller.ViewModels;

internal class RecipeViewModel : ObservableObject, IQueryAttributable
{
    private Models.Recipe _recipe;
    private Models.UnitOfMeasurement _yieldUnitOfMeasurement = null;

    public long ID =>
        _recipe is null ? 0 : _recipe.ID;

    public DateTime CreatedAt =>
        _recipe is null ? DateTime.MinValue : _recipe.CreatedAt;

    public DateTime UpdatedAt =>
        _recipe is null ? DateTime.MinValue : _recipe.UpdatedAt;

    public string Title {
        get { return _recipe.Name; }
        set {
            if (_recipe.Name != value) {
                _recipe.Name = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    public double Yield {
        get { return _recipe.Yield; }
        set {
            if (_recipe.Yield != value) {
                _recipe.Yield = value;
                OnPropertyChanged(nameof(Yield));
            }
        }
    }

    public string YieldUnit {
        get { return _recipe.YieldUnit; }
        set {
            if (value != "" && !UnitsOfMeasurement.Instance.ByName.ContainsKey(value))
                return;

            if (_recipe.YieldUnit != value) {
                _recipe.YieldUnit = value;
                SetYieldUnitOfMeasurement();
                OnPropertyChanged(nameof(YieldUnit));
            }
        }
    }

    public string YieldUnitAbbreviation =>
        _yieldUnitOfMeasurement is null ? "" : _yieldUnitOfMeasurement.Abbreviation;

    public string YieldUnitKind => 
        _yieldUnitOfMeasurement is null ? "" : _yieldUnitOfMeasurement.Kind;

    public string YieldUnitSystem => 
        _yieldUnitOfMeasurement is null ? "" : _yieldUnitOfMeasurement.System;

    public string Description {
        get { return _recipe.Description; }
        set {
            if (_recipe.Description != value) {
                _recipe.Description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    public ObservableCollection<RecipeIngredientViewModel> Ingredients { get; }

    public ICommand ConvertYieldUnitCommand { get; }

    public RecipeViewModel() : this(new Models.Recipe("Production"))
    {
    }

    public RecipeViewModel(Models.Recipe r)
    {
        _recipe = r;
        Ingredients = new ObservableCollection<RecipeIngredientViewModel>();
        SetYieldUnitOfMeasurement();
        SetIngredients();
        ConvertYieldUnitCommand = new RelayCommand<string>(ConvertYieldUnit);
    }

    public void ConvertYieldUnit(string toUnit)
    {
        if (_yieldUnitOfMeasurement is null)
            throw new ApplicationException("Cannot convert an empty unit of measurement");

        if (!UnitsOfMeasurement.Instance.ByName.ContainsKey(toUnit))
            throw new ArgumentException($"No unit of measurement named: {toUnit}");
        UnitOfMeasurement to = UnitsOfMeasurement.Instance.ByName[toUnit];

        if (_yieldUnitOfMeasurement.Kind != to.Kind)
            throw new ApplicationException($"Cannot convert {_yieldUnitOfMeasurement.Kind} to {to.Kind}");

        double newYield = _yieldUnitOfMeasurement.Convert(Yield, to);
        Yield = newYield;
        YieldUnit = to.Name;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        throw new NotImplementedException();
    }

    private void SetYieldUnitOfMeasurement()
    {
        if (_recipe.YieldUnit == "")
            _yieldUnitOfMeasurement = null;
        else
            _yieldUnitOfMeasurement = UnitsOfMeasurement.Instance.ByName[_recipe.YieldUnit];

        OnPropertyChanged(nameof(YieldUnitAbbreviation));
        OnPropertyChanged(nameof(YieldUnitKind));
        OnPropertyChanged(nameof(YieldUnitSystem));
    }

    private void SetIngredients()
    {
        if (_recipe is null || _recipe.ID == 0) {
            Ingredients.Clear();
            return;
        }

        Ingredients.Clear();
        List<RecipeIngredientViewModel> lst = new List<RecipeIngredientViewModel>(
            Models.RecipeIngredient.LoadAllForRecipe(_recipe.ID)
                .Select(i => new RecipeIngredientViewModel(i)));
        foreach (RecipeIngredientViewModel i in lst)
            Ingredients.Add(i);
    }
}