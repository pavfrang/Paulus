function initHotTopics(element) {	
	return new HotTopics(element);
}

var HotTopics = function(element) {
	var self = this;
	var element = element;
	var bAnim = false;
	
	var aPrev = (searchChilds(element, "prev"))[0];
	var aNext = (searchChilds(element, "next"))[0];
	
	var topics_list = (searchChilds(element, "topics_list"))[0];
	var topics_list_width = getWidth(topics_list,false,false,true);
	
	var gr_l = (searchChilds(topics_list, "gr_l"))[0];
	var gr_r = (searchChilds(topics_list, "gr_r"))[0];
	
	var ul = (searchChildsByTagName(topics_list, "UL"))[0];
	var li = searchChildsByTagName(ul, "LI");
	
	var items = new Array();
	var steps = new Array();
	var cur_step = complete_step = 0;
	var w = 0;
	
	steps[0]=0;
	for (var i=0,j=1,x=0,n=li.length ; i < n ; i++) {
		items[i] = {};
		items[i].w = getWidth(li[i],true,true,true);
		items[i].x = w;
		w += items[i].w;
		
		if (x + items[i].w > topics_list_width) {
			steps[j] = items[i].x;
			x=0;
			j++;
		} else {
			x += items[i].w;
		}
	}
	ul.style.width = w+"px";
	
	//last step
	j=steps.length-1;
	steps[j] = w - topics_list_width;
	
	var initGrState = function() {
		//var l = getStyleValue(ul, "left");
		if (cur_step > 0) {
			gr_l.style.display = "block";
			removeClass(aPrev,"nav_disable");
		} else {
			gr_l.style.display = "none";
			addClass(aPrev,"nav_disable");
		}
		if (cur_step < steps.length-1) {
			gr_r.style.display = "block";
			removeClass(aNext,"nav_disable");
		} else {
			gr_r.style.display = "none";
			addClass(aNext,"nav_disable");
		}
	};
	
	var from_left, to_left;
	var ease = "swing";
	var anim_step = function(state,n,d) {
		var diff = to_left - from_left;
		var x = to_left;
		
		if (state >= 1) {
			x = from_left + diff;
		} else {
			if (ease == "swing") {
				x = ((-Math.cos(state*Math.PI)/2) + 0.5) * diff + from_left;
			} else if (ease == "easeOutExpo") {
				if (n==d) {
					x = 1;
				} else {
					x = 1 * (-Math.pow(2, -10 * n/d) + 1);
					x = from_left + diff * x;
				}
			} else {
				x = from_left + diff * state;
			}
		}
		ul.style.left = x + "px";
	};
	
	var anim_complete = function(state) {
		cur_step = complete_step;
		bAnim = false;
		initGrState();
	};
	
	var aPrev_click = function() {
		if (cur_step > 0 && !bAnim) {
			bAnim = true;
			complete_step = cur_step-1;
			from_left = steps[cur_step] * -1;
			to_left = steps[complete_step] * -1;
			
			new Animation(600,anim_step,anim_complete);
		}
		return false;
	};
	
	var aNext_click = function() {
		if (cur_step < steps.length-1 && !bAnim) {
			bAnim = true;
			complete_step = cur_step+1;
			from_left = steps[cur_step] * -1;
			to_left = steps[complete_step] * -1;
			
			new Animation(600,anim_step,anim_complete);
		}
		return false;
	};
	
	
	aPrev.onclick = aPrev_click;
	aNext.onclick = aNext_click;
	
	initGrState();
};