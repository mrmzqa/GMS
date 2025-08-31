using CommunityToolkit.Mvvm.ComponentModel;
using GMSApp.Models.job;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GMSApp.Models;
public class ItemRow : ObservableObject
{
    [Key]
    public int Id { get; set; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                OnPropertyChanged(nameof(Total));
        }
    }

    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
                OnPropertyChanged(nameof(Total));
        }
    }

    private decimal _price;
    public decimal Price
    {
        get => _price;
        set
        {
            if (SetProperty(ref _price, value))
                OnPropertyChanged(nameof(Total));
        }
    }

    [NotMapped]
    public decimal Total => Quantity * Price;

    // FK to Joborder.Id
    public int JoborderId { get; set; }

    // Navigation property
    public GMSApp.Models.job.Joborder? Joborder { get; set; }
}


