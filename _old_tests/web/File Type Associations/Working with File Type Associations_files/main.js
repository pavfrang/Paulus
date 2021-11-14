$(document).ready(function(){
	
	
	aux_ready();
	init_login_popup();
	
	$('input.star').rating({
		readOnly: true,
		split: 4
	});
	
	//$('input.article_rating').rating();
	/*
	$("ul#menu").supersubs({ 
		minWidth:    1,   // minimum width of sub-menus in em units 
		maxWidth:    60,   // maximum width of sub-menus in em units 
		extraWidth:  1     // extra width can ensure lines don't sometimes turn over 
                           // due to slight rounding differences and font-family 
		}).superfish();  // call supersubs first, then superfish, so that subs are 
                         // not display:none when measuring. Call before initialising 
                         // containing tabs for same reason.
						 
						 */
	//
	/*
	$("ul#menu").supersubs({
		rolloverColor: 	'#FFFFFF',
		outColor: 		'#000000',
		minWidth:    1,   // minimum width of sub-menus in em units 
		maxWidth:    60,   // maximum width of sub-menus in em units 
		extraWidth:  1     // extra width can ensure lines don't sometimes turn over 
                           // due to slight rounding differences and font-family 
		}).superfish();  // call supersubs first, then superfish, so that subs are 
                         // not display:none when measuring. Call before initialising 
                         // containing tabs for same reason.
						 
	*/
	init_login();
	//$("ul#menu").supersubs();
});

function aux_ready() {
}

function showRequest(req){
	setAlpha("div.bottom_menu", 0.3);
}

function loginResponse(resp){
	if(resp.success){
		//window.location.href = "/index.php"; 
		//window.location.href = window.location.href;
		$("div#popupContact form div.invalid_login").hide();
		if($("input#pagetype").length && $("input#pagetype").val() == "community_article" && $("input#articleid").length){
			getArticleRating($("input#articleid").val());
		}
		/*
		$.get("/logintopmenu.php", function response(data){
			//window.location.href = window.location.href;
			setAlpha("div.bottom_menu", 1);
			alert(data);
			refreshPageAfterLoginLogout(data);
			disablePopup();
			login_process();
		});
		*/
		
		setAlpha("div.bottom_menu", 1);
		refreshPageAfterLoginLogout(resp.html);
		disablePopup();
		login_process();
	}else{
		setAlpha("div.bottom_menu", 1);
		if(!$("div#popupContact form div.invalid_login").length){
			$("div#popupContact form input[type='submit']").after('<div class="invalid_login">Invalid login.</div>');
		}else{
			$("div#popupContact form div.invalid_login").show();
		}
		
	}
}
	
function setAlpha(path, alpha){
	if($.browser.msie){
		$(path).css('filter', 'alpha(opacity = '+(alpha*100)+')'); 
		$(path).css('background', 'white'); 
	}else{
		$(path).css('opacity', alpha); 
	}
}
function init_login(){
	var options = { 
        target:        '#output2',   // target element(s) to be updated with server response 
        beforeSubmit:  showRequest,  // pre-submit callback 
        success:       loginResponse,  // post-submit callback 
		dataType:  		'json'
        // other available options: 
        //url:       url         // override for form's 'action' attribute 
        //type:      type        // 'get' or 'post', override for form's 'method' attribute 
        //dataType:  null        // 'xml', 'script', or 'json' (expected server response type) 
        //clearForm: true        // clear all form fields after successful submit 
        //resetForm: true        // reset the form after successful submit 

        // $.ajax options can be used here too, for example: 
        //timeout:   3000 
    }; 
	$("div#popupContact form div.invalid_login").hide();
    // bind to the form's submit event 
    $('div#popupContact form').submit(function() { 
        $(this).ajaxSubmit(options);
        return false; 
    });
}







function refreshPageAfterLoginLogout(data){
	if($("input#pagetype").length && $("input#pagetype").val() == "community_article" && $("input#articleid").length){
		getArticleRating($("input#articleid").val());
	}
	$("div#headerBottomSecondNav div.bottom_menu ul").html(data);
	$("div#headerBottomSecondNav div.bottom_menu ul a#logout_link").unbind('click').bind('click', logout);
	$("div#headerBottomSecondNav div.bottom_menu ul a#login_link").unbind('click').bind('click', login);
}

function logout(){
	//$.get("")
	setAlpha("div.bottom_menu", 0.3);
	$.post("/ajax_handler.php", { handler_name: "logout"}, function response(data){
		//$.get("/logintopmenu.php", function response(data){
			//window.location.href = window.location.href;
			if(data.success){
				refreshPageAfterLoginLogout(data.html);
				disablePopup();
				logout_process();
			}
			setAlpha("div.bottom_menu", 1);
		//});
	}, "json");
}

window.logout_process = function(){
}

window.login_process = function(){
}

function login(){
	//centering with css
	centerPopup();
	//load popup
	loadPopup();
}
//CONTROLLING EVENTS IN jQuery
function init_login_popup(){
	//$('div#popupContact').corners({ inColor: '#FEFF8F', outColor: '#666666' });
	
	/*
	$("div#headerBottomSecondNav a#login_link").unbind('click');
	$("div#headerBottomSecondNav a#login_link").bind('click', login);
	
	$("div#headerBottomSecondNav a#logout_link").unbind('click');
	$("div#headerBottomSecondNav a#logout_link").bind('click', logout);
	*/
	
	//CLOSING POPUP
	//Click the x event!
	$("#popupContactClose").click(function(){
		disablePopup();
	});
	//Click out event!
	$("#backgroundPopup").click(function(){
		disablePopup();
	});
	//Press Escape event!
	$(document).keypress(function(e){
		if(e.keyCode==27 && popupStatus==1){
			disablePopup();
		}
	});
}

function getCaptchaForm(imageid){
	var img = '<img width="300" height="100" onload="$(\'.captcha_refresh\').attr(\'disabled\', false);" class="captcha_verifyimg" src="/icom_cgi/freud/getimg.pcgi?csid='+imageid+'"/>';
	var str = "<div>Please enter the phrase in this image into the textbox below.</div>";
	str += "<div>"+img+"</div>";
	str += "<div><input type='text' name='passphrase' size='7' /><input type='hidden' name='csid' value='"+imageid+"' /><img src='/newimg/images/repeat.png' class='captcha_image' onclick='reloadCaptcha(this);' style='cursor: pointer;' /></div>";
	str += "<div style='width: 99%; text-align: left;'>In deciphering this image, you are verifying your identity as an actual site user (a human being--not a bot!). <br/> Thanks for helping us protect our system.</div>";
	str += "<div></div>";
	return str;
}


function reloadCaptcha(ob){
	$.post("/icom_cgi/freud/getcsid.cgi", {}, function (resp){
		$(ob.parentNode.parentNode).html(getCaptchaForm(resp));
	});
}


function getArticleRating(articleid){

$.get("/ratings/rating_form.php", {articleid: articleid},
	function(response){
		//alert(response.form);
		$("div#article_vote_wrapper").html(response.form);
		if(response.readonly){
			$('input.article_rating').rating({
				readOnly: true
			});
		}else{
			$('input.article_rating').rating({
				callback: function(value, link){
					// 'this' is the hidden form element holding the current value
					// 'value' is the value selected
					// 'element' points to the link element that received the click.
					if(!value){
						value = 0;
					}
					articleid = $("input#articleid").val();
					//alert(value + " : "+value);
					$.get("/ratings/post_rating.php", { vote: value, split: 4, articleid: articleid}, function response(data){
						$("div#vote_response").html(data.message);
							getArticleRating(articleid);
							if(data.success){}else{}
					});

					//alert(this + " : "+$(this));

					$('input.article_rating').rating('disable');
				}
			});
		}
	}, "json");
}


pageTracker = new Object();
pageTracker._initData = function(){};
pageTracker._trackPageview = function(){};
