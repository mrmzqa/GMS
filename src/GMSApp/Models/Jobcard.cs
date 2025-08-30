namespace GMSApp.Models;
public class Jobcard
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public DateTime JobDate { get; set; }
    public string JobDescription { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public List<ItemRow> Items { get; set; } = new();
    public DateTime? CompletionDate { get; set; }
    public byte[] Propertyimage { get; set; }
}