# r-calculation-engine
A calculation engine using RDOTNET to discuss how one might implement.

Discussion Points - 


Architecture

The architecture should probably be that we can start a number of these calculation nodes within a managed architecture. 
So we split the work across tickers

Data Tier

We split the data tier into a factory with providers we can just plug-in as necessary - all nicely testable.

R Libs - We need to provide a library of calculation functions. Do we just want to use R.NET or do we want to run some R-Scripts within
SQL 2017 with the relevant permissions and external_script?

# Angular SPA and Rest Services/WCF

We design a web services layer so that the Angular Application can communicate with it using all the latest tools..
We use DevExpress or equivalent and D3 to create financial based charts, manage state using ngrx/localstorage and use pwa
data can be cached. My preference is to use Async REST services with observables on the client side.

Authentication strategy - Azure, JWT, or on prem OAUTH.

Anymore discussion points that come up...
