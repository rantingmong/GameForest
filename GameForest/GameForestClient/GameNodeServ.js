var express = require('express');
var app = express();

var trackable = [
	{ stat : 'Dummy', val : 0}
]

app.use(express.bodyParser());

app.get('/GameNodeServ', function(req, res) {
	res.json(trackable);
});

app.get('/GameNodeServ/:id', function(req, res) {
	if(trackable.length <= req.params.id || req.params.id < 0) {
    res.statusCode = 404;
    return res.send('Error 404: No trackable stat found');
  	}

  	var s = trackable[req.params.id];
  	res.json(s);
});

app.post('/GameNodeServ', function(req, res) {
	if(!req.body.hasOwnProperty('stat') || 
    !req.body.hasOwnProperty('val')) {
    res.statusCode = 400;
    return res.send('Error 400: Post syntax incorrect.');
  	}

  	var newTrackable = {
  		stat : req.body.stat,
  		val : req.body.val
  	}

  	trackable.push(newTrackable);
  	res.json(true);
});

app.delete('/GameNodeServ/:id', function(req, res) {
  if(trackable.length <= req.params.id) {
    res.statusCode = 404;
    return res.send('Error 404: No trackable stat found');
  }

  trackable.splice(req.params.id, 1);
  res.json(true);
});

app.listen(process.env.PORT || 8888);