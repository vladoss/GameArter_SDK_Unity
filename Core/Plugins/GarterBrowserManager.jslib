mergeInto(LibraryManager.library, {
    
    _gb_initGameSignature: function(dataObj){
    	try{
    		new GarterUnityGame().init(JSON.parse(Pointer_stringify(dataObj)));
    	} catch(e){
    		console.log("E1 BM| ", Pointer_stringify(dataObj), e);
    	}
    },

    _gb_gameInitialized: function(state){
        try{
    		Garter.game.gameInitialized(Pointer_stringify(state));
    	} catch(e){
    		console.log("E2 BM| ", Pointer_stringify(state),  e);
    	}
    },

    _gb_adRequest: function(dataObj){
        try{
    		Garter.game.ads(JSON.parse(Pointer_stringify(dataObj)));
    	} catch(e){
    		console.log("E3 BM| ", Pointer_stringify(dataObj), e);
    	}
    },

    _gb_analyticsRequest: function(dataObj){ // problem of first call
        try{
    		if(Garter.game !== undefined){
	            Garter.game.analytics (JSON.parse(Pointer_stringify(dataObj)));
	        } else {
	            setTimeout(function() {
	               try{
	                    Garter.game.analytics (JSON.parse(Pointer_stringify(dataObj))); // game init
	               } catch(e){
	                    console.error("GARTER| ", e);
	               }
	            }, 2000);
	        }
    	} catch(e){
    		console.log("E4 BM| ", Pointer_stringify(dataObj), e);
    	}
    },

    _gb_openModuleWindow: function(data){
        try{
    		Garter.game.openWindow(JSON.parse(Pointer_stringify(data)));
    	} catch(e){
    		console.log("E5 BM| ", Pointer_stringify(data), e);
    	}
    },

    _gb_storyAnimation: function(num){
		try{
    		Garter.game.runStoryAnimation(num);
    	} catch(e){
    		console.log("E6 BM| ", num, e);
    	}
    },

    _gb_userBilanceCallback: function(data){
        try{
    		Garter.game.userBilanceCallback(JSON.parse(Pointer_stringify(data)));
    	} catch(e){
    		console.log("E7 BM| ", Pointer_stringify(data), e);
    	}
    },

    _gb_activityPing: function(activityState){
        try{
    		Garter.game.active (activityState);
    	} catch(e){
    		console.log("E8 BM| ", Pointer_stringify(activityState), e);
    	}
    }, 

    _gb_gameSettings: function(data){  
        try{
    		Garter.game.settingsInfo(JSON.parse(Pointer_stringify(data)));
    	} catch(e){
    		console.log("E9 BM| ", Pointer_stringify(data), e);
    	}
    },

    _gb_gameRestart: function(){
        try{
    		Garter.game.restart();
    	} catch(e){
    		console.log("E10 BM| ", e);
    	}
    },

    _gb_gameBan: function(data){
    	try{
    		Garter.game.ban(JSON.parse(Pointer_stringify(data)));
    	} catch(e){
    		console.log("E11 BM| ", Pointer_stringify(data), e);
    	}
    },
    
    _gb_fullscreen: function() {
        document.onmouseup = function()
        {
            Pointer_stringify();
        	try{
        		if((window.fullScreen) || (window.innerWidth == screen.width && window.innerHeight == screen.height)) { 
        			if (document.cancelFullScreen) { document.cancelFullScreen();
					} else if ( document.mozCancelFullScreen) { document.mozCancelFullScreen();
					} else if (document.webkitCancelFullScreen) { document.webkitCancelFullScreen(); }
				} else {
					var el = document.getElementsByTagName("html")[0],
				    	rfs = el.requestFullscreen || el.webkitRequestFullScreen || el.mozRequestFullScreen || el.msRequestFullscreen;
					rfs.call(el);
				}
			} catch(e){}
        	document.onmouseup = null;
        }
    },

    _gb_openWindow: function(link) {
        var url = Pointer_stringify(link);
        document.onmouseup = function()
        {
        	window.open(url);
        	document.onmouseup = null;
        }
    },

    _gb_installAsPwa: function() {
        document.onmouseup = function()
        {
            Pointer_stringify();
            try {
            	Garter.game.installAsPwa();
            } catch(e){console.error(e);}
        	document.onmouseup = null;
        }
    },

});