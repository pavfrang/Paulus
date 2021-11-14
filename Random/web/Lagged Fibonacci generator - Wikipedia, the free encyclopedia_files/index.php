/* generated javascript */
var skin = 'monobook';
var stylepath = '/skins-1.5';

/* MediaWiki:Common.js */
//<source lang="javascript">

/* Import more specific scripts if necessary */

if (wgAction == "edit" || wgAction == "submit") //scripts specific to editing pages
{
    importScript("MediaWiki:Common.js/edit.js")
}
else if (wgPageName == "Special:Watchlist") //watchlist scripts
{
    importScript("MediaWiki:Common.js/watchlist.js")
}
else if (wgPageName == "Special:Search") //scripts specific to Special:Search
{
    importScript("MediaWiki:Common.js/search.js")
}


/** Sysop Javascript *******************************************************
  *
  *  Description: Allows for sysop-specific Javascript at [[MediaWiki:Sysop.js]].
  */
function sysopFunctions() {
    if ( wgUserGroups && !window.disableSysopJS ) {
        for ( var g = 0; g < wgUserGroups.length; ++g ) {
            if ( wgUserGroups[g] == "sysop" ) {
                importScript( "MediaWiki:Sysop.js" );
                break;
            }
        }
    }
}

addOnloadHook( sysopFunctions );


/** WikiMiniAtlas *******************************************************
  *
  *  Description: WikiMiniAtlas is a popup click and drag world map.
  *               This script causes all of our coordinate links to display the WikiMiniAtlas popup button.
  *               The script itself is located on meta because it is used by many projects.
  *               See [[Meta:WikiMiniAtlas]] for more information. 
  *  Maintainers: [[User:Dschwen]]
  */

if (wgServer == "https://secure.wikimedia.org") {
  var metaBase = "https://secure.wikimedia.org/wikipedia/meta";
} else {
  var metaBase = "http://meta.wikimedia.org";
}
importScriptURI(metaBase+"/w/index.php?title=MediaWiki:Wikiminiatlas.js&action=raw&ctype=text/javascript&smaxage=21600&maxage=86400")


/* Scripts specific to Internet Explorer */

if (navigator.appName == "Microsoft Internet Explorer")
{
    /** Internet Explorer bug fix **************************************************
     *
     *  Description: Fixes IE horizontal scrollbar bug
     *  Maintainers: [[User:Tom-]]?
     */
    
    var oldWidth;
    var docEl = document.documentElement;
    
    function fixIEScroll()
    {
        if (!oldWidth || docEl.clientWidth > oldWidth)
            doFixIEScroll();
        else
            setTimeout(doFixIEScroll, 1);
        
        oldWidth = docEl.clientWidth;
    }
    
    function doFixIEScroll() {
        docEl.style.overflowX = (docEl.scrollWidth - docEl.clientWidth < 4) ? "hidden" : "";
    }
    
    document.attachEvent("onreadystatechange", fixIEScroll);
    document.attachEvent("onresize", fixIEScroll);
    
    
    /**
     * Remove need for CSS hacks regarding MSIE and IPA.
     */
    
    if (document.createStyleSheet) {
        document.createStyleSheet().addRule('.IPA', 'font-family: "Doulos SIL", "Charis SIL", Gentium, "DejaVu Sans", Code2000, "TITUS Cyberbit Basic", "Arial Unicode MS", "Lucida Sans Unicode", "Chrysanthi Unicode";');
    }
    
    
    //Import scripts specific to Internet Explorer 6
    if (navigator.appVersion.substr(22, 1) == "6")
    {
        importScript("MediaWiki:Common.js/IE60Fixes.js")
    }
}


/* Test if an element has a certain class **************************************
 *
 * Description: Uses regular expressions and caching for better performance.
 * Maintainers: [[User:Mike Dillon]], [[User:R. Koot]], [[User:SG]]
 */

var hasClass = (function () {
    var reCache = {};
    return function (element, className) {
        return (reCache[className] ? reCache[className] : (reCache[className] = new RegExp("(?:\\s|^)" + className + "(?:\\s|$)"))).test(element.className);
    };
})();


/** Interwiki links to featured articles ***************************************
 *
 *  Description: Highlights interwiki links to featured articles (or
 *               equivalents) by changing the bullet before the interwiki link
 *               into a star.
 *  Maintainers: [[User:R. Koot]]
 */

function LinkFA() 
{
    if ( document.getElementById( "p-lang" ) ) {
        var InterwikiLinks = document.getElementById( "p-lang" ).getElementsByTagName( "li" );

        for ( var i = 0; i < InterwikiLinks.length; i++ ) {
            if ( document.getElementById( InterwikiLinks[i].className + "-fa" ) ) {
                InterwikiLinks[i].className += " FA"
                InterwikiLinks[i].title = "This is a featured article in another language.";
            }
        }
    }
}

addOnloadHook( LinkFA );


/** Collapsible tables *********************************************************
 *
 *  Description: Allows tables to be collapsed, showing only the header. See
 *               [[Wikipedia:NavFrame]].
 *  Maintainers: [[User:R. Koot]]
 */

var autoCollapse = 2;
var collapseCaption = "hide";
var expandCaption = "show";

function collapseTable( tableIndex )
{
    var Button = document.getElementById( "collapseButton" + tableIndex );
    var Table = document.getElementById( "collapsibleTable" + tableIndex );

    if ( !Table || !Button ) {
        return false;
    }

    var Rows = Table.rows;

    if ( Button.firstChild.data == collapseCaption ) {
        for ( var i = 1; i < Rows.length; i++ ) {
            Rows[i].style.display = "none";
        }
        Button.firstChild.data = expandCaption;
    } else {
        for ( var i = 1; i < Rows.length; i++ ) {
            Rows[i].style.display = Rows[0].style.display;
        }
        Button.firstChild.data = collapseCaption;
    }
}

function createCollapseButtons()
{
    var tableIndex = 0;
    var NavigationBoxes = new Object();
    var Tables = document.getElementsByTagName( "table" );

    for ( var i = 0; i < Tables.length; i++ ) {
        if ( hasClass( Tables[i], "collapsible" ) ) {

            /* only add button and increment count if there is a header row to work with */
            var HeaderRow = Tables[i].getElementsByTagName( "tr" )[0];
            if (!HeaderRow) continue;
            var Header = HeaderRow.getElementsByTagName( "th" )[0];
            if (!Header) continue;

            NavigationBoxes[ tableIndex ] = Tables[i];
            Tables[i].setAttribute( "id", "collapsibleTable" + tableIndex );

            var Button     = document.createElement( "span" );
            var ButtonLink = document.createElement( "a" );
            var ButtonText = document.createTextNode( collapseCaption );

            Button.style.styleFloat = "right";
            Button.style.cssFloat = "right";
            Button.style.fontWeight = "normal";
            Button.style.textAlign = "right";
            Button.style.width = "6em";

            ButtonLink.style.color = Header.style.color;
            ButtonLink.setAttribute( "id", "collapseButton" + tableIndex );
            ButtonLink.setAttribute( "href", "javascript:collapseTable(" + tableIndex + ");" );
            ButtonLink.appendChild( ButtonText );

            Button.appendChild( document.createTextNode( "[" ) );
            Button.appendChild( ButtonLink );
            Button.appendChild( document.createTextNode( "]" ) );

            Header.insertBefore( Button, Header.childNodes[0] );
            tableIndex++;
        }
    }

    for ( var i = 0;  i < tableIndex; i++ ) {
        if ( hasClass( NavigationBoxes[i], "collapsed" ) || ( tableIndex >= autoCollapse && hasClass( NavigationBoxes[i], "autocollapse" ) ) ) {
            collapseTable( i );
        }
    }
}

addOnloadHook( createCollapseButtons );


/** Dynamic Navigation Bars (experimental) *************************************
 *
 *  Description: See [[Wikipedia:NavFrame]].
 *  Maintainers: UNMAINTAINED
 */

// set up the words in your language
var NavigationBarHide = '[' + collapseCaption + ']';
var NavigationBarShow = '[' + expandCaption + ']';

// shows and hides content and picture (if available) of navigation bars
// Parameters:
//     indexNavigationBar: the index of navigation bar to be toggled
function toggleNavigationBar(indexNavigationBar)
{
    var NavToggle = document.getElementById("NavToggle" + indexNavigationBar);
    var NavFrame = document.getElementById("NavFrame" + indexNavigationBar);

    if (!NavFrame || !NavToggle) {
        return false;
    }

    // if shown now
    if (NavToggle.firstChild.data == NavigationBarHide) {
        for (var NavChild = NavFrame.firstChild; NavChild != null; NavChild = NavChild.nextSibling) {
            if ( hasClass( NavChild, 'NavPic' ) ) {
                NavChild.style.display = 'none';
            }
            if ( hasClass( NavChild, 'NavContent') ) {
                NavChild.style.display = 'none';
            }
        }
    NavToggle.firstChild.data = NavigationBarShow;

    // if hidden now
    } else if (NavToggle.firstChild.data == NavigationBarShow) {
        for (var NavChild = NavFrame.firstChild; NavChild != null; NavChild = NavChild.nextSibling) {
            if (hasClass(NavChild, 'NavPic')) {
                NavChild.style.display = 'block';
            }
            if (hasClass(NavChild, 'NavContent')) {
                NavChild.style.display = 'block';
            }
        }
        NavToggle.firstChild.data = NavigationBarHide;
    }
}

// adds show/hide-button to navigation bars
function createNavigationBarToggleButton()
{
    var indexNavigationBar = 0;
    // iterate over all < div >-elements 
    var divs = document.getElementsByTagName("div");
    for (var i = 0; NavFrame = divs[i]; i++) {
        // if found a navigation bar
        if (hasClass(NavFrame, "NavFrame")) {

            indexNavigationBar++;
            var NavToggle = document.createElement("a");
            NavToggle.className = 'NavToggle';
            NavToggle.setAttribute('id', 'NavToggle' + indexNavigationBar);
            NavToggle.setAttribute('href', 'javascript:toggleNavigationBar(' + indexNavigationBar + ');');

            var NavToggleText = document.createTextNode(NavigationBarHide);
            for (var NavChild = NavFrame.firstChild; NavChild != null; NavChild = NavChild.nextSibling) {
                if ( hasClass( NavChild, 'NavPic' ) || hasClass( NavChild, 'NavContent' ) ) {
                    if (NavChild.style.display == 'none') {
                        NavToggleText = document.createTextNode(NavigationBarShow);
                        break;
                    }
                }
            }

            NavToggle.appendChild(NavToggleText);
            // Find the NavHead and attach the toggle link (Must be this complicated because Moz's firstChild handling is borked)
            for(var j=0; j < NavFrame.childNodes.length; j++) {
                if (hasClass(NavFrame.childNodes[j], "NavHead")) {
                    NavFrame.childNodes[j].appendChild(NavToggle);
                }
            }
            NavFrame.setAttribute('id', 'NavFrame' + indexNavigationBar);
        }
    }
}

addOnloadHook( createNavigationBarToggleButton );


/** Main Page layout fixes *********************************************************
 *
 *  Description: Adds an additional link to the complete list of languages available.
 *  Maintainers: [[User:AzaToth]], [[User:R. Koot]], [[User:Alex Smotrov]]
 */

function mainPageAppendCompleteListLink() {
    addPortletLink('p-lang', 'http://meta.wikimedia.org/wiki/List_of_Wikipedias',
                   'Complete list', 'interwiki-completelist', 'Complete list of Wikipedias')
}

if ( wgTitle == 'Main Page' && wgNamespaceNumber == 0 ) {
    addOnloadHook( mainPageAppendCompleteListLink );
}


/** "Technical restrictions" title fix *****************************************
 *
 *  Description: For pages that have something like Template:Lowercase, replace
 *               the title, but only if it is cut-and-pasteable as a valid
 *               wikilink. For instance, iPod's title is updated, but [[C#]]
 *               is not an equivalent wikilink, so [[C Sharp]] doesn't have its
 *               main title changed. Likewise, for users who have selected the
 *               UK date format ("1 March"), the  titles of day-of-the-year 
 *               articles will appear in that style. Users with any other date
 *               setting are not affected.
 *
 *               The function looks for a banner like this: 
 *               <div id="RealTitleBanner"> ... <span id="RealTitle">title</span> ... </div>
 *               An element with id=DisableRealTitle disables the function.
 *  Maintainers: [[User:Interiot]], [[User:Mets501]], [[User:Freakofnurture]]
 */

var disableRealTitle = 0; // users can set disableRealTitle = 1 locally to disable.
if (wgIsArticle) { // don't display the RealTitle when editing, since it is apparently inconsistent (doesn't show when editing sections, doesn't show when not previewing)
 addOnloadHook(function() {
    try {
        var realTitleBanner = document.getElementById("RealTitleBanner");
        if (realTitleBanner && !document.getElementById("DisableRealTitle") && !disableRealTitle ) {
            var realTitle = document.getElementById("RealTitle");
            if (realTitle) {
                var realTitleHTML = realTitle.innerHTML;
                if (realTitle.textContent) { //everyone but IE
                    realTitleText = realTitle.textContent
                } else { //IE
                    realTitleText = realTitle.innerText;
                }

                var isPasteable = 0;
                var containsTooMuchHTML = /</.test( realTitleHTML.replace(/<\/?(sub|sup|small|big)>/gi, "") ); // contains HTML that will be ignored when cut-n-pasted as a wikilink
                // calculate whether the title is pasteable
                var verifyTitle = realTitleText.replace(/^ +/, "");       // trim left spaces
                verifyTitle = verifyTitle.charAt(0).toUpperCase() + verifyTitle.substring(1, verifyTitle.length);    // uppercase first character

                // if the namespace prefix is there, remove it on our verification copy. If it isn't there, add it to the original realValue copy.
                if (wgNamespaceNumber != 0) {
                    if (wgCanonicalNamespace == verifyTitle.substr(0, wgCanonicalNamespace.length).replace(/ /g, "_") && verifyTitle.charAt(wgCanonicalNamespace.length) == ":") {
                        verifyTitle = verifyTitle.substr(wgCanonicalNamespace.length + 1);
                    } else {
                        realTitleText = wgCanonicalNamespace.replace(/_/g, " ") + ":" + realTitleText;
                        realTitleHTML = wgCanonicalNamespace.replace(/_/g, " ") + ":" + realTitleHTML;
                    }
                }

                // verify whether wgTitle matches
                verifyTitle = verifyTitle.replace(/[\s_]+/g, " ");      // underscores and multiple spaces to single spaces
                verifyTitle = verifyTitle.replace(/^\s+/, "").replace(/\s+$/, "");        // trim left and right spaces
                verifyTitle = verifyTitle.charAt(0).toUpperCase() + verifyTitle.substring(1, verifyTitle.length);    // uppercase first character
                if ( (verifyTitle == wgTitle) || (verifyTitle == wgTitle.replace(/^(.+)?(January|February|March|April|May|June|July|August|September|October|November|December)\s+([12]?[0-9]|3[0123])([^\d].*)?$/g, "$1$3 $2$4") )) isPasteable = 1;
                var h1 = document.getElementsByTagName("h1")[0];
                if (h1 && isPasteable) {
                    h1.innerHTML = containsTooMuchHTML ? realTitleText : realTitleHTML;
                    if (!containsTooMuchHTML)
                        realTitleBanner.style.display = "none";
                }
                document.title = realTitleText + " - Wikipedia, the free encyclopedia";
            }
        }
    } catch (e) {
        /* Something went wrong. */
    }
 });
}


/** Anon tips and donation banner **************************
  *
  *  Description: This implements an anon tips / donation banner. It includes a workaround for
  *               the Z-index bug found in Internet Explorer. It correctly places the anon notice
  *               on the page, even under IE6. See this Google search for more information about the bug:
  *               http://www.google.com/search?q=z-index+ie6+bug
  *  Maintainers: [[User:Gmaxwell]], [[User:MZMcBride]]
  */

if(wgUserName == null && skin == 'monobook') addOnloadHook((function (){
    var message=new Array();
        message[0]='Your <a href="http://wikimediafoundation.org/wiki/Fundraising?source=enwiki_00" class="extiw" title="wikimedia:Fundraising"><b>continued donations</b></a> keep Wikipedia running!';
        message[1]='<a href="http://wikimediafoundation.org/wiki/Fundraising?source=enwiki_01" class="extiw" title="foundation:Fundraising"><b>Make a donation</b></a> to Wikipedia and give the gift of knowledge!';
        message[2]='Wikipedia is sustained by people like you. Please <a href="http://wikimediafoundation.org/wiki/Fundraising?source=enwiki_02" class="extiw" title="foundation:fundraising"><b>donate</b></a> today.';
        message[3]='Help us improve Wikipedia by <a href="http://wikimediafoundation.org/wiki/Fundraising?source=enwiki_03" class="extiw" title="foundation:Fundraising"><b>supporting it financially</b></a>.';
        message[4]='You can <a href="http://wikimediafoundation.org/wiki/Fundraising?source=enwiki_04" class="extiw" title="wikimedia:Fundraising"><b>support Wikipedia</b></a> by making a tax-deductible donation.'
        message[5]='Help us provide free content to the world by <a href="http://wikimediafoundation.org/wiki/Fundraising?source=enwiki_05" class="extiw" title="foundation:Fundraising"><b>donating today</b></a>!';
        message[6]='<a href="http://en.wikipedia.org/wiki/Wikipedia:Researching_with_Wikipedia" title="Wikipedia:Researching with Wikipedia">Learn more about using Wikipedia for research.</a>';
        message[7]='<a href="http://en.wikipedia.org/wiki/Wikipedia:Ten_things_you_may_not_know_about_Wikipedia" title="Wikipedia:Ten things you may not know about Wikipedia">Ten things you may not know about Wikipedia.</a>';
        message[8]='<a href="http://en.wikipedia.org/wiki/Wikipedia:Ten_things_you_may_not_know_about_images_on_Wikipedia" title="Wikipedia:Ten things you may not know about images on Wikipedia">Ten things you may not know about images on Wikipedia.</a>';
        message[9]='<a href="http://en.wikipedia.org/wiki/Wikipedia:Citing_Wikipedia" title="Wikipedia:Citing Wikipedia">Learn more about citing Wikipedia.</a>';
        message[10]='Have questions? <a href="http://en.wikipedia.org/wiki/Wikipedia:Questions" title="Wikipedia:Questions">Find out how to ask questions and get answers.</a>';
        message[11]='<a href="http://en.wikipedia.org/wiki/Wikipedia:Basic_navigation" title="Wikipedia:Basic navigation">Find out more about navigating Wikipedia and finding information.</a>';
        message[12]='<a href="http://en.wikipedia.org/wiki/Wikipedia:Contributing_to_Wikipedia" title="Wikipedia:Contributing to Wikipedia">Interested in contributing to Wikipedia?</a>';
    var weightLimit = 6;
    var biasPercent = 0.815;
    var whichMessage = (Math.random() < biasPercent) ? weightLimit : message.length;
 
    whichMessage = Math.floor(Math.random() * whichMessage);
 
    var wrapper = document.getElementById("globalWrapper");
    if (wrapper) {
        var div = document.createElement('div');
        div.id = "anon-banner";
        div.style.cssText = "position:absolute; z-index:40; left:155px; top:1px; clear:both; float:left; font-size:90%; font-style:italic; white-space:nowrap";
        div.innerHTML = message[whichMessage];
        wrapper.insertBefore(div, wrapper.firstChild);
    }
}));


/** Secure upload link fix ************************************************
  *
  *  Description: Fix "Upload file" link when using the secure proxy.
  *               This is a workaround that can be removed when [[bugzilla:10843]] is fixed.
  *  Maintainers: [[User:Remember the dot]]
  */

addOnloadHook(function()
{
    if (document.getElementById("t-upload"))
    {
        document.getElementById("t-upload").getElementsByTagName("a")[0].href = wgArticlePath.replace("$1", "Wikipedia:Upload")
    }
})


/** Table sorting fixes ************************************************
  *
  *  Description: Disables code in table sorting routine to set classes on even/odd rows
  *  Maintainers: [[User:Random832]]
  */

ts_alternate_row_colors = false;


/** Modernista referrer ************************************************
  *
  *  Description: Adds a notice to pages viewed through modernista.com
  *  Maintainers: [[User:Random832]]
  */

addOnloadHook(function(){
  if(/modernista\.com/.test(document.referrer)) {
	 jsMsg('<table><tr><td><img src="http://upload.wikimedia.org/'
	 +'wikipedia/commons/thumb/d/dc/Nuvola_apps_important_yellow.svg/'
	 +'48px-Nuvola_apps_important_yellow.svg.png" /></td><td><br /><br />You '
	 +'appear to have come here from the Modernista website. They '
	 +'enclose Wikipedia’s content with a frame and overlay their '
	 +'own navigation banner on top. Wikipedia does not endorse '
	 +'Modernista and its appearance here should not be taken to '
	 +'imply this.</td></tr></table>');
  }
});

/***** uploadwizard_newusers ********
 * Switches in a message for non-autoconfirmed users at [[Wikipedia:Upload]]
 *
 *  Maintainers: [[User:Krimpet]]
 ****/
function uploadwizard_newusers() {
  if (wgNamespaceNumber == 4 && wgTitle == "Upload" && wgAction == "view") {
    var oldDiv = document.getElementById("autoconfirmedusers"),
        newDiv = document.getElementById("newusers");
    if (oldDiv && newDiv && typeof wgUserGroups == "object" && wgUserGroups) {
      for (i = 0; i < wgUserGroups.length; i++) {
        if (wgUserGroups[i] == "autoconfirmed") {
          oldDiv.style.display = "block";
          newDiv.style.display = "none";
          return;
        }
      }

      oldDiv.style.display = "none";
      newDiv.style.display = "block";
    }
  }
}
addOnloadHook(uploadwizard_newusers);


/** IPv6 AAAA connectivity testing *******************************************************
  *
  *  Description: Uses hidden images to measure the possible negative impact of IPv6
  *  enabling the Wikimedia sites.
  *  This works by adding a hidden div to the footer with several image tags. 
  *  The source addresses of the image tags are set to domainnames which have v4, v6 and
  *  both address types set.  The script times how long objects take to load.
  *  Results are sent back to the server. http://ipv6and4.labs.wikimedia.org/stats.html
  *  Based on http://www.braintrust.co.nz/ipv6wwwtest/
  *  Contact: [[User:Gmaxwell]], [[User:Mark Bergsma]], [[User:Mindspillage]]
  */

var __ipv6wwwtest_factor = 100;
var __ipv6wwwtest_done = 0;
if ((wgServer != "https://secure.wikimedia.org") && (Math.floor(Math.random()*__ipv6wwwtest_factor)==42)) {
        var __ipv6wwwtest_timeoutMsec = 10000; // Timeout for 'final' result message in milliseconds
        var __ipv6wwwtest_hostSuffix = ".labs.wikimedia.org"; // Suffix to go on the IMG hostnames
        var __ipv6wwwtest_stopAtTimeout = true; // Whether to stop when the timeout is reached or not

        var __ipv6wwwtest_pageLoadTime;
        var __ipv6wwwtest_timeout = false;
        var __ipv6wwwtest_ipv4LoadTime = false;
        var __ipv6wwwtest_ipv4relLoadTime = false;
        var __ipv6wwwtest_ipv6LoadTime = false;
        var __ipv6wwwtest_ipv6bigLoadTime = false;
        var __ipv6wwwtest_ipv6and4LoadTime = false;
        var __ipv6wwwtest_id = Math.floor(Math.random()*Math.pow(2,31));

        function __ipv6wwwtest_startTest() {
                __ipv6wwwtest_pageLoadTime = new Date();
                document.getElementById("__ipv6wwwtest_ipv4Img").src = "http://ipv4" + __ipv6wwwtest_hostSuffix +"/ipv4.gif?id=" + __ipv6wwwtest_id;
                document.getElementById("__ipv6wwwtest_ipv4relImg").src = "//ipv4" + __ipv6wwwtest_hostSuffix +"/ipv4.gif?rel=1&id=" + __ipv6wwwtest_id;
                document.getElementById("__ipv6wwwtest_ipv6Img").src = "http://ipv6" + __ipv6wwwtest_hostSuffix +"/ipv6.gif?id=" + __ipv6wwwtest_id;
                document.getElementById("__ipv6wwwtest_ipv6and4Img").src = "http://ipv6and4" + __ipv6wwwtest_hostSuffix +"/ipv6and4.gif?id=" + __ipv6wwwtest_id;
                document.getElementById("__ipv6wwwtest_ipv6bigImg").src = "http://ipv6" + __ipv6wwwtest_hostSuffix +"/ipv6big.gif?id=" + __ipv6wwwtest_id;
        }

        function __ipv6wwwtest_sendResults(stage) {
                document.getElementById("__ipv6wwwtest_resultsImg").src = "http://results" + __ipv6wwwtest_hostSuffix +"/results.gif?id=" + __ipv6wwwtest_id + "&stage=" + stage + "&timeout=" + __ipv6wwwtest_timeoutMsec + "&stop_at_timeout=" + __ipv6wwwtest_stopAtTimeout + "&ipv4=" + __ipv6wwwtest_getLoadTime(__ipv6wwwtest_ipv4LoadTime) + "&ipv6=" + __ipv6wwwtest_getLoadTime(__ipv6wwwtest_ipv6LoadTime) + "&ipv6and4=" + __ipv6wwwtest_getLoadTime(__ipv6wwwtest_ipv6and4LoadTime) + "&ipv6big=" + __ipv6wwwtest_getLoadTime(__ipv6wwwtest_ipv6bigLoadTime) +"&ipv4rel="+ __ipv6wwwtest_getLoadTime(__ipv6wwwtest_ipv4relLoadTime) + "&rate=" + __ipv6wwwtest_factor;
        };

        function __ipv6wwwtest_getLoadTime(item) {
                if (item == false) {
                        return "NaN";
                } else {
                        return (item.getTime() - __ipv6wwwtest_pageLoadTime.getTime());
                }
        }

        function __ipv6wwwtest_checkFinished() {
                if ( (! __ipv6wwwtest_ipv6LoadTime) || (! __ipv6wwwtest_ipv4LoadTime) || (! __ipv6wwwtest_ipv6and4LoadTime) || (! __ipv6wwwtest_ipv6bigLoadTime) || (! __ipv6wwwtest_getLoadTime)) {
                        if (!__ipv6wwwtest_timeout) {
                                __ipv6wwwtest_timeout = window.setTimeout('__ipv6wwwtest_sendFinalResults()',__ipv6wwwtest_timeoutMsec);
                        }
                        __ipv6wwwtest_sendResults('partial');
                } else {
                        __ipv6wwwtest_sendFinalResults();
                }
        }

        function __ipv6wwwtest_sendFinalResults() {
                if (__ipv6wwwtest_done==0) {
                  if (__ipv6wwwtest_timeout) {
                          window.clearTimeout(__ipv6wwwtest_timeout);
                  }
                  __ipv6wwwtest_sendResults('final');

                  if (__ipv6wwwtest_stopAtTimeout) {
                          document.getElementById("__ipv6wwwtest_ipv4Img").src = "";
                          document.getElementById("__ipv6wwwtest_ipv4relImg").src = "";
                          document.getElementById("__ipv6wwwtest_ipv6Img").src = "";
                          document.getElementById("__ipv6wwwtest_ipv6and4Img").src = "";
                          document.getElementById("__ipv6wwwtest_ipv6bigImg").src = "";
                  }
                }
                __ipv6wwwtest_done=1;
        }
  addOnloadHook(function() {
        v6sub=document.getElementById("footer");
        v6sub.innerHTML=v6sub.innerHTML+'<div style="visibility: hidden;"> <img height="1" width="1" src="" id="__ipv6wwwtest_ipv4Img" onload="__ipv6wwwtest_ipv4LoadTime = new Date(); __ipv6wwwtest_checkFinished();" /> <img height="1" width="1" src="" id="__ipv6wwwtest_ipv4relImg" onload="__ipv6wwwtest_ipv4relLoadTime = new Date(); __ipv6wwwtest_checkFinished();" /> <img height="1" width="1" src="" id="__ipv6wwwtest_ipv6and4Img" onload="__ipv6wwwtest_ipv6and4LoadTime = new Date(); __ipv6wwwtest_checkFinished();" /> <img height="1" width="1" src="" id="__ipv6wwwtest_ipv6Img" onload="__ipv6wwwtest_ipv6LoadTime = new Date(); __ipv6wwwtest_checkFinished();" /> <img height="1" width="1" src="" id="__ipv6wwwtest_ipv6bigImg" onload="__ipv6wwwtest_ipv6bigLoadTime = new Date(); __ipv6wwwtest_checkFinished();" /> <img height="1" width="1" src="" id="__ipv6wwwtest_resultsImg" /> </div>';
       if (document.getElementById("__ipv6wwwtest_ipv4Img") && document.getElementById("__ipv6wwwtest_ipv6Img") && document.getElementById("__ipv6wwwtest_ipv6and4Img") && document.getElementById("__ipv6wwwtest_ipv6bigImg")) {
         __ipv6wwwtest_startTest();
       }
   });
}

//</source>

/* MediaWiki:Monobook.js */
/* JavaScript here will be loaded for users using the MonoBook skin only. */