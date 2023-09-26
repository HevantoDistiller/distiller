using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Distiller.Models;

namespace Distiller.ViewModels;

internal class RecipeIngredientViewModel : ObservableObject, IQueryAttributable
{
    private Models.RecipeIngredient _ingredient;
    private Models.UnitOfMeasurement _unitOfMeasurement;

    public string Name {
        get { return _ingredient.Name; }
        set {
            if (_ingredient.Name != value) {
                _ingredient.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public double Amount {
        get { return _ingredient.Amount; }
        set {
            if (_ingredient.Amount != value) {
                _ingredient.Amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }
    }

    public string Unit {
        get { return _ingredient.Unit; }
        set {
            if (value != "" && !UnitsOfMeasurement.Instance.ByName.ContainsKey(value))
                return;
            
            if (_ingredient.Unit != value) {
                _ingredient.Unit = value;
                SetUnitOfMeasurement();
                OnPropertyChanged(nameof(Unit));
            }
        }
    }

    public string UnitAbbreviation => 
        _unitOfMeasurement is null ? "" : _unitOfMeasurement.Abbreviation;

    public string UnitKind => 
        _unitOfMeasurement is null ? "" : _unitOfMeasurement.Kind;

    public string UnitSystem => 
        _unitOfMeasurement is null ? "" : _unitOfMeasurement.System;
        
    public ICommand ConvertUnitCommand { get; }

    public RecipeIngredientViewModel()
    {

    }

    public RecipeIngredientViewModel(Models.RecipeIngredient i)
    {
        _ingredient = i;
        SetUnitOfMeasurement();
        ConvertUnitCommand = new RelayCommand<string>(ConvertUnit);
    }

    public void ConvertUnit(string toUnit)
    {
        if (_unitOfMeasurement is null)
            throw new ApplicationException("Cannot convert an empty unit of measurement");

        if (!UnitsOfMeasurement.Instance.ByName.ContainsKey(toUnit))
            throw new ApplicationException($"No unit of measurement named: {toUnit}");
        UnitOfMeasurement to = UnitsOfMeasurement.Instance.ByName[toUnit];

        if (_unitOfMeasurement.Kind != to.Kind)
            throw new ApplicationException($"Cannot convert {_unitOfMeasurement.Kind} to {to.Kind}");
        
        double newAmount = _unitOfMeasurement.Convert(Amount, to);
        Amount = newAmount;
        Unit = to.Name;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        throw new NotImplementedException();
    }

    private void SetUnitOfMeasurement()
    {
        if (_ingredient.Unit == "")
            _unitOfMeasurement = null;
        else
            _unitOfMeasurement = UnitsOfMeasurement.Instance.ByName[_ingredient.Unit];

        OnPropertyChanged(nameof(UnitAbbreviation));
        OnPropertyChanged(nameof(UnitKind));
        OnPropertyChanged(nameof(UnitSystem));
    }
}