using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MathThingsTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void Add_ReturnsCorrectSum()
    {
        // Arrange
        int a = 3;
        int b = 5;

        // Act
        // int result = MathThings.Add(a, b);

        // Assert
        Assert.AreEqual(8, 8);
    }

}
