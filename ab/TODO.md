Backend
-------
[ ] Alternative class should be used in Experiment class for consistency 
[ ] Exclude robots from experiments
[ ] Persistence layer using adapter pattern and multiple listeners (like Console, Database, etc.)
[ ] Hash of metric increments by date (count is based on daily values only)
[ ] Class cleanup and separation
[ ] Remote metrics support (for plugins)
[ ] Google Analytics integration

UI
--
[ ] Force conclude an experiment
[ ] Show/hide internal AB metrics from metrics dashboard
[ ] Warning when there are concluded experiments in the DB that aren't in the registry
[ ] All metrics with default viz types
[ ] Transition to knockout vs. server-side template rendering
[ ] Realtime updates with SignalR