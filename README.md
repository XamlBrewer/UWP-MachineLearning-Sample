# UWP-MachineLearning-Sample
Demonstrates how to use ML.NET and Oxyplot to implement some Machine Learning use cases in an MVVM UWP app. 

## Look mom, no Python
The UWP app demonstrates the following Machine Learning scenarios. Focus is currently on the ML.NET core functionality, not on its latest API:
* Clustering
* Multiclass Classification
* Binary Classification (in progress)
* Regression (todo)
* Recommendation (todo)
* Feature Analysis with Boxplot Diagrams

## Seeing is believing

![Screenshot](Assets/Clustering.png?raw=true)

![Screenshot](Assets/MulticlassClassification.png?raw=true)

![Screenshot](Assets/BinaryClassification.png?raw=true)

![Screenshot](Assets/BoxPlot.png?raw=true)

## What could possibly go wrong?
We had to implement some workarounds because today's ML.NET release is in beta and temporarily not very UWP-friendly: 
* it uses Reflection.Emit (blocked by UWP in Release mode), 
* it uses the MEF composition container (not exposed to UWP by CoreFX), and
* it does file and process manipulations on classic API's that are not available in UWP because of the sandbox.