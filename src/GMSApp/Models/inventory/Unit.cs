using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.inventory
{
    /// <summary>
    /// Enumeration of measurement units including standard metrics and common garage/workshop BOM units.
    /// </summary>
    public enum Unit
    {
        // Standard Measurement Units
        // Length
        Millimeter = 1,
        Centimeter = 2,
        Meter = 3,
        Inch = 4,
        Foot = 5,

        // Mass/Weight
        Gram = 6,
        Kilogram = 7,
        Pound = 8,

        // Volume
        Milliliter = 9,
        Liter = 10,
        CubicMeter = 11,
        Gallon = 12,

        // Area
        SquareMeter = 13,
        SquareFoot = 14,

        // Time
        Second = 15,
        Minute = 16,
        Hour = 17,

        // Temperature
        Celsius = 18,
        Fahrenheit = 19,

        // BOM / Workshop-Specific Units (common in material lists and construction)
        Piece = 20,       // e.g., "PC"
        Bag = 21,         // e.g., cement in bags
        Roll = 22,        // e.g., wire or tape
        Sheet = 23,       // e.g., metal sheet or drywall
        Tube = 24,
        Box = 25,
        Carton = 26,
        Bundle = 27,
        Pair = 28,
        SET = 29,
        Unit = 30,        // e.g., "UN" for generic unit
    }



}
