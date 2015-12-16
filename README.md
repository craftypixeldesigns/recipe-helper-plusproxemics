# recipe-helper-plusproxemics
Prototype Code: A proxemic display using the Microsoft Kinect. Optimizing the recipe flow for two novice chefs. For a school assignment

# To run:
##[wpf](https://github.com/missCarrieMah/recipe-helper-plusproxemics/tree/master/wpf)
* Incorporates gestures and proxemics
* Requires Visual Studio 2013 or better
* Requires a Miccrosoft Kinect 2
* Go to RecipeHelperProxemics.sln (or to view all files go to the RecipeHelperProxemics folder)
* Run from MainWindow.xaml

##[reveal.js](https://github.com/missCarrieMah/recipe-helper-plusproxemics/tree/master/reveal.js)
* Only used to illustrate the screens
* Requires a browser (optimal on Google Chrome)
* Requires the [Leap Motion](https://developer.leapmotion.com/) SDK
* Requires [Node.js](https://nodejs.org/en/) & [Grunt](http://gruntjs.com/getting-started) to run [Reveal.js](https://github.com/hakimel/reveal.js)
* In the [reveal.js folder](https://github.com/missCarrieMah/recipe-helper/tree/master/reveal.js), run `grunt serve`
* Plug in your leap motion, ensure you're in a lighted area and you're ready to go!

## Gestures & proxemics:

See [ins1.png](https://github.com/missCarrieMah/recipe-helper-proxemics/blob/master/recipe-helper-master/reveal.js/img/ins1.png) and [ins2.png](https://github.com/missCarrieMah/recipe-helper-proxemics/blob/master/recipe-helper-master/reveal.js/img/ins2.png) for diagrams

Note: this is for the RecipeHelperProxemics

Proximity between people
* Social Proximity: being separated (to test, move towards the left of the Kinect device) will split the instructions into two sides
* Personal Proximity: being close (to test, move towards the right of the Kinect device) will merge the instructions into the current step
* Intimate Proximity: extending the arm changes the steps (left arm for previous step, right arm for next step)

Proximity between person and screen
* Intimate Proximity: tapping the button Saad/Jack will assign a step
* Personal Proximity: raising the arm gets additional information
