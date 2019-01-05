# UWP-MachineLearning-Sample
Demonstrates how to use ML.NET to implement some Machine Learning use cases in UWP. 

## What could possibly go wrong?
Today's ML.NET release is not too UWP-friendly: 
* it uses Reflection.Emit (blocked by UWP in Release mode), 
* it uses the MEF composition container (not exposed to UWP by CoreFX), and
* it does file and process manipulations on classic API's that are not available in UWP because of the sandbox.

So a lot can go wrong, but we believe this situation is temporary.

## Look mom, no Python
The UWP app demonstrates the following Machine Learning scenarios. Focus is currently on the ML.NET core functionality, not on its latest API:
* Clustering
* Multiclass Classification
* Binary Classification (in progress)
* Regression (todo)
* Recommendation (todo)

## Seeing is believing

![Screenshot](Assets/Clustering.png?raw=true)

![Screenshot](Assets/MulticlassClassification.png?raw=true)

![Screenshot](Assets/BinaryClassification.png?raw=true)