var tabs = new Array();

oldOnLoad = window.onload;
if (typeof oldOnLoad == "function") {
	window.onload = function() {
		oldOnLoad();
		initTabs();
	};
} else {
	window.onload = function() {
		initTabs();
	};
}

var initTabs = function() {
	all = document.getElementsByTagName("DIV");
	var j = 0;
	for(var i=0;i<all.length;i++) {
		if (hasClass(all[i],"tabs")) {
			
			var menu = searchChilds(all[i],"tabsMenu");
			var contents = searchChilds(all[i],"tabsContent");
			
			if (menu.length > 0 && contents.length > 0) {
				tabs[j] = {};
				tabs[j].e = all[i];
				tabs[j].menu = menu;
				tabs[j].contents = menu;
				tabs[j].current = undefined;
				j++;
			}
		}
	}
	
	for(var i=0;i<tabs.length;i++) {
		
		
		var t = new function() {
			var tab = tabs[i];
			for(var j=0;j<tab.menu.length;j++) {
				var menus = tab.menu[j].getElementsByTagName('A');
				for(var m=0;m<menus.length;m++) {
					if (hasClass(menus[m],"selected")) {
						showTab(tab,menus[m]);
					}
					menus[m].onclick = function() {
						hideTab(tab);
						showTab(tab,this);
						return false;
					}
				}
			}
		}
		
		
	}
}

var showTab = function(tab,obj) {
	var h = obj.href;
	var a = h.split("#",2);
	if (a.length > 1) {
		addClass(obj,"selected");
		document.getElementById(a[1]).style.display="block";
	}
	tab.current = obj;
	
}

var hideTab = function(tab) {
	var obj = tab.current;
	var h = obj.href;
	var a = h.split("#",2);
	if (a.length > 1) {
		removeClass(obj,"selected");
		removeClass(document.getElementById(a[1]),"selected");
		document.getElementById(a[1]).style.display="none";
	}
	
}
