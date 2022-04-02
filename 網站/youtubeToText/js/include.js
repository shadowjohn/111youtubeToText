//複製URL地址
var userAgent = navigator.userAgent.toLowerCase();
var is_opera = userAgent.indexOf('opera') != -1 && opera.version();
var is_moz = (navigator.product == 'Gecko') && userAgent.substr(userAgent.indexOf('firefox') + 8, 3);
var is_ie = (userAgent.indexOf('msie') != -1 && !is_opera) && userAgent.substr(userAgent.indexOf('msie') + 5, 3);
var is_safari = (userAgent.indexOf('webkit') != -1 || userAgent.indexOf('safari') != -1);
/*
  //iframe包含
  if (top.location != location) {
  	top.location.href = location.href;
  }

  function $(id) {
	 return document.getElementById(id);
  }
*/

function setCopy(_sTxt) {
    if (is_ie) {
        clipboardData.setData('Text', _sTxt);
        alert("網址「" + _sTxt + "」\n已經複製到您的剪貼板中\n您可以使用Ctrl+V快捷鍵粘貼到需要的地方");
    } else {
        prompt("請複製網站地址:", _sTxt);
    }
}


String.prototype.trim = function () { return this.replace(/(^\s*)|(\s*$)/g, "") };

function handleEnter(field, event) {
    // 按 enter 不會 submit onkeypress="return handleEnter(this, event);"         
    var keyCode = event.keyCode ? event.keyCode : event.which ? event.which : event.charCode;
    if (keyCode == 13) {
        var i;
        for (i = 0; i < field.form.elements.length; i++)
            if (field == field.form.elements[i])
                break;
        i = (i + 1) % field.form.elements.length;
        field.form.elements[i].focus();
        return false;
    }
    else
        return true;
}


function execInnerScript(innerhtml) {
    var temp = innerhtml.replace(/\n|\r/g, "");
    var regex = /<script.+?<\/script>/gi;
    var arr = temp.match(regex);
    if (arr) {
        for (var iiiiiiiiii_iii = 0; iiiiiiiiii_iii < arr.length; iiiiiiiiii_iii++) {
            var temp1 = arr[iiiiiiiiii_iii];
            var reg = new RegExp("^<script(.+?)>(.+)<\/script>$", "gi");
            reg.test(temp1);
            eval(RegExp.$2);
        }
    }
}



function getWindowSize() {
    var myWidth = 0, myHeight = 0;
    if (typeof (window.innerWidth) == 'number') {
        //Non-IE
        myWidth = window.innerWidth;
        myHeight = window.innerHeight;
    } else if (document.documentElement && (document.documentElement.clientWidth || document.documentElement.clientHeight)) {
        //IE 6+ in 'standards compliant mode'
        myWidth = document.documentElement.clientWidth;
        myHeight = document.documentElement.clientHeight;
    } else if (document.body && (document.body.clientWidth || document.body.clientHeight)) {
        //IE 4 compatible
        myWidth = document.body.clientWidth;
        myHeight = document.body.clientHeight;
    }
    var a = new Object();
    a['width'] = myWidth;
    a['height'] = myHeight;
    return a;
}

/*
	//給jQuery擴充的 cookie 功能
		http://www.stilbuero.de/2006/09/17/cookie-plugin-for-jquery/

	jQuery操作cookie的插件,大概的使用方法如下

	设置cookie的值
			$.cookie('the_cookie', ‘the_value');
	新建一个cookie 包括有效期 路径 域名等
			$.cookie('the_cookie', ‘the_value', {expires: 7, path: ‘/', domain: ‘jquery.com', secure: true});
	新建cookie
		  $.cookie('the_cookie', ‘the_value');
	删除一个cookie
		  $.cookie('the_cookie', null);

*/
jQuery.cookie = function (name, value, options) {
    if (typeof value != 'undefined') { // name and value given, set cookie
        options = options || {};
        if (value === null) {
            value = '';
            options.expires = -1;
        }
        var expires = '';
        if (options.expires && (typeof options.expires == 'number' || options.expires.toUTCString)) {
            var date;
            if (typeof options.expires == 'number') {
                date = new Date();
                date.setTime(date.getTime() + (options.expires * 24 * 60 * 60 * 1000));
            } else {
                date = options.expires;
            }
            expires = '; expires=' + date.toUTCString(); // use expires attribute, max-age is not supported by IE
        }
        var path = options.path ? '; path=' + options.path : '';
        var domain = options.domain ? '; domain=' + options.domain : '';
        var secure = options.secure ? '; secure' : '';
        document.cookie = [name, '=', encodeURIComponent(value), expires, path, domain, secure].join('');
    } else { // only name given, get cookie
        var cookieValue = null;
        if (document.cookie && document.cookie != '') {
            var cookies = document.cookie.split(';');
            for (var i = 0; i < cookies.length; i++) {
                var cookie = jQuery.trim(cookies[i]);
                // Does this cookie string begin with the name we want?
                if (cookie.substring(0, name.length + 1) == (name + '=')) {
                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                    break;
                }
            }
        }
        return cookieValue;
    }
};

//我的ajax
function myAjax(url, postdata) {
    var tmp = $.ajax({
        url: url,
        type: "POST",
        data: postdata,
        async: false
    }).responseText;
    return tmp;
}
function myAjax_async(url, postdata, func) {
    $.ajax({
        url: url,
        type: "POST",
        data: postdata,
        async: true,
        success: function (html) {
            func(html);
        }
    });
}
//自然滑動機
function div_motion(domid) {
    var pre_name = "3WA_" + new Date().getTime();

    window[pre_name + 'paperdown'] = 0;
    window[pre_name + 'motion'] = 0;
    $("#" + domid).mousedown(function (event) {
        if (window[pre_name + 'paperdown'] != 0) {
            $("#" + domid).stop();
        }
        window[pre_name + 'startmovetime'] = new Date().getTime();
        window[pre_name + 'paperdown'] = 1;
        window[pre_name + 'moveX'] = event.pageX;
        window[pre_name + 'moveY'] = event.pageY;
        window[pre_name + 'motion'] = 0;
    });
    $("#" + domid).mouseup(function () {
        window[pre_name + 'endmovetime'] = new Date().getTime();
        window[pre_name + 'lastX'] = this.scrollLeft;
        window[pre_name + 'lastY'] = this.scrollTop;
        var orz = window[pre_name + 'endmovetime'] - window[pre_name + 'startmovetime'];
        if (orz >= 15) {
            orz = 0;
            window[pre_name + 'paperdown'] = 0;
            window[pre_name + 'motion'] = 0;
        }
        else {
            orz = 15 - orz;
            window[pre_name + 'motion'] = 1;
            $("#" + domid).animate({
                'scrollLeft': (window[pre_name + 'lastX'] + (window[pre_name + 'lastX'] - window[pre_name + 'moveSX']) / 0.1),
                'scrollTop': (window[pre_name + 'lastY'] + (window[pre_name + 'lastY'] - window[pre_name + 'moveSY']) / 0.05)
            }, {
                    duration: orz * 100,
                    query: false,
                    complete: function () {
                        window[pre_name + 'paperdown'] = 0;
                        window[pre_name + 'motion'] = 0;
                    }
                });
        }
    });
    $("#" + domid).mousemove(function (event) {
        if (window[pre_name + 'paperdown'] == 1 && window[pre_name + 'motion'] == 0) {
            window[pre_name + 'startmovetime'] = new Date().getTime();
            window[pre_name + 'moveSX'] = this.scrollLeft;
            window[pre_name + 'moveSY'] = this.scrollTop;

            this.scrollLeft -= (event.pageX - window[pre_name + 'moveX']);
            this.scrollTop -= (event.pageY - window[pre_name + 'moveY']);
            window[pre_name + 'moveX'] = event.pageX;
            window[pre_name + 'moveY'] = event.pageY;
        }
    });
}

function disableEnterKey(e) {
    var key;

    if (window.event)
        key = window.event.keyCode;     //IE
    else
        key = e.which;     //firefox

    if (key == 13)
        return false;
    else
        return true;
}

function comment(input, width, height) {
    $("#mycolorbox").html(input).dialog({
        'width': width,
        'height': height
    });
}

function ValidEmail(emailtoCheck) {
    //  email
    //  規則:  1.只有一個  "@"
    //              2.網址中,  至少要有一個".",  且不能連續出現
    //              3.不能有空白
    var regExp = /^[^@^\s]+@[^\.@^\s]+(\.[^\.@^\s]+)+$/;
    if (emailtoCheck.match(regExp))
        return true;
    else
        return false;
}
function ValidPhone(phonenum) {
    //格式為 09xx-xxxxxx
    var tel = /^(09)\d{2}-\d{6}$/;
    if (!tel.test(phonenum)) {
        return false;
    }
    else {
        return true;
    }
}

jQuery.fn.outerHTML = function (s) {
    return (s) ? $(this).replaceWith(s) : $(this).clone().wrap('<p>').parent().html();
}

//scroll page to id , 如 #id
function Animate2id(dom) {
    var animSpeed = 800; //animation speed
    var easeType = "easeInOutExpo"; //easing type
    if ($.browser.webkit) { //webkit browsers do not support animate-html
        $("body").stop().animate({ scrollTop: $(dom).offset().top }, animSpeed, easeType, function () {
            $(dom).focus();
        }
        );
    } else {
        $("html").stop().animate({ scrollTop: $(dom).offset().top }, animSpeed, easeType, function () {
            $(dom).focus();
        }
        );
    }
}
function size_hum_read($size) {
    /* Returns a human readable size */
    $size = parseInt($size);
    var $i = 0;
    var $iec = new Array();
    var $iec_kind = "B,KB,MB,GB,TB,PB,EB,ZB,YB";
    $iec = explode(',', $iec_kind);
    while (($size / 1024) > 1) {
        $size = $size / 1024;
        $i++;
    }
    return sprintf("%s %s", substr($size, 0, strpos($size, '.') + 3), $iec[$i]);
}
$(document).ready(function () {
    //mouse_init();

});
function plus_or_minus_one_month($year_month, $kind) {
    //$year_month 傳入值格式為 2011-01
    //$kind 看是 '+' or '-'
    //回傳格式為 2011-01  
    switch ($kind) {
        case '+':
            return date('Y-m', strtotime("+1 month", strtotime($year_month)));
            break;
        case '-':
            return date('Y-m', strtotime("-1 month", strtotime($year_month)));
            break;
    }
}
function my_ids_mix(ids) {
    var m = new Array();
    m = explode(",", ids);
    var data = new Array();
    for (i = 0, max_i = m.length; i < max_i; i++) {
        array_push(data, m[i] + "=" + encodeURIComponent($("#" + m[i]).val()));
    }
    return implode('&', data);
}
function my_names_mix(indom) {
    var m = new Array();
    var names = $(indom).find('*[req="group[]"]');
    for (i = 0, max = names.length; i < max; i++) {
        array_push(m, $(names[i]).attr('name') + "=" + encodeURIComponent($(names[i]).val()));
    }
    return implode('&', m);
}

function anti_right_click() {
    //鎖右鍵防盜
    document.onselectstart = function () { return false; }
    document.ondragstart = function () { return false; }
    document.oncontextmenu = function () { return false; }
    if (document.all)
        document.body.onselectstart = function () { return false; };
    else {
        $('body').css('-moz-user-select', 'none');
        $('body').css('-webkit-user-select', 'none');
    }
    document.onmousedown = clkARR_;
    document.onkeydown = clkARR_;
    document.onkeyup = clkARRx_;
    window.onmousedown = clkARR_;
    window.onkeydown = clkARR_;
    window.onkeyup = clkARRx_;

    var clkARRCtrl = false;

    function clkARRx_(e) {
        var k = (e) ? e.which : event.keyCode;
        if (k == 17) clkARRCtrl = false;
    }

    function clkARR_(e) {
        var k = (e) ? e.which : event.keyCode;
        var m = (e) ? (e.which == 3) : (event.button == 2);
        if (k == 17) clkARRCtrl = true;
        if (m || clkARRCtrl && (k == 67 || k == 83))
            alert((typeof (clkARRMsg) == 'string') ? clkARRMsg : '-版權所有-請勿複製-');
    }
}
function dialogMyBoxOn(message, op, functionAction) {
    $.fn.center = function () {
        this.css("position", "absolute");
        this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
        this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
        return this;
    }
    $.mybox({
        is_background_touch_close: op,
        message: message,
        css: {
            border: '2px solid #fff',
            backgroundColor: '#fff',
            color: '#000',
            padding: '15px'
        },
        onBlock: function () {
            functionAction();
        }
    });
}
function dialogMyBoxOff() {
    $.unmybox();
}
function basename(filepath) {
    $m = explode("/", filepath);
    return end($m);
}
function subname(filepath) {
    $m = explode(".", $fname);
    return end($m);
}
function getext($s) { return strtolower(subname($s)); }
function isvideo($file) { if (in_array(getext($file), new Array('mpg', 'mpeg', 'avi', 'rm', 'rmvb', 'mov', 'wmv', 'mod', 'asf', 'm1v', 'mp2', 'mpe', 'mpa', 'flv', '3pg', 'vob'))) { return true; } return false; }
function isdocument($file) { if (in_array(getext($file), new Array('docx', 'odt', 'odp', 'ods', 'odc', 'csv', 'doc', 'txt', 'pdf', 'ppt', 'pps', 'xls'))) { return true; } return false; }
function isimage($file) { if (in_array(getext($file), new Array('jpg', 'bmp', 'gif', 'png', 'jpeg', 'tiff', 'tif', 'psd'))) { return true; } return false; }
function isspecimage($file) { if (in_array(getext($file), new Array('tiff', 'tif', 'psd'))) { return true; } return false; }
function isweb($file) { if (in_array(getext($file), new Array('htm', 'html'))) { return true; } return false; }
function iscode($file) { if (in_array(getext($file), new Array('c', 'cpp', 'h', 'pl', 'py', 'php', 'phps', 'asp', 'aspx', 'css', 'jsp', 'sh', 'shar'))) { return true; } return false; }

function print_table($ra, $fields, $headers, $classname) {
    $classname = (typeof ($classname) == "undefined" || $classname == '') ? '' : " class='" + $classname + "' ";
    if (typeof ($fields) == "undefined" || $fields == '' || $fields == '*') {

        $tmp = sprintf("<table %s border='1' cellspacing='0' cellpadding='0'>", $classname);
        $tmp += "<thead><tr>";
        for (var k in $ra[0]) {
            $tmp += sprintf("<th field=\"" + k + "\">%s</th>", k);
        }
        $tmp += "</tr></thead>";
        $tmp += "<tbody>";
        for ($i = 0, $max_i = count($ra); $i < $max_i; $i++) {
            $tmp += "<tr>";
            for (var k in $ra[$i]) {
                $tmp += sprintf("<td field=\"%s\">%s</td>", k, $ra[$i][k]);
            }
            $tmp += "</tr>";
        }
        $tmp += "</tbody>";
        $tmp += "</table>";
        return $tmp;
    }
    else {
        $tmp = sprintf("<table %s border='1' cellspacing='0' cellpadding='0'>", $classname);
        $tmp += "<thead><tr>";
        $mheaders = explode(',', $headers);
        $m_fields = $fields.split(',');
        for (var k in $mheaders) {
            $tmp += sprintf("<th field=\"" + $m_fields[k] + "\">%s</th>", $mheaders[k]);
        }
        $tmp += "</tr></thead>";
        $tmp += "<tbody>";
        $m_fields = explode(',', $fields);
        for ($i = 0, $max_i = count($ra); $i < $max_i; $i++) {
            $tmp += "<tr>";
            for (var k in $m_fields) {
                $tmp += sprintf("<td field=\"%s\">%s</td>", $m_fields[k], $ra[$i][$m_fields[k]]);
            }
            $tmp += "</tr>";
        }
        $tmp += "</tbody>";
        $tmp += "</table>";
        return $tmp;
    }
}
function getCheckBox_val(dom_name) {
    //return array
    /*var arr = new Array();
    for (var i = 0, max_i = $($("*[name='" + dom_name + "']")).length; i < max_i; i++) {
        if ($($("*[name='" + dom_name + "']")[i]).prop('checked')) {
            array_push(arr, $($("*[name='" + dom_name + "']")[i]).val());
        }
    }*/

    var arr = new Array();
    var doms = $("input[name='" + dom_name + "']:checked");
    for (i = 0, max_i = doms.length; i < max_i; i++) {
        array_push(arr, doms.eq(i).val());
    }
    return arr;
}
function getCheckBoxSelector_val(dom) {
    var arr = new Array();
    var doms = $(dom + ":checked");
    for (i = 0, max_i = doms.size(); i < max_i; i++) {
        array_push(arr, doms.eq(i).val());
    }
    return arr;
}
function is_string_like($data, $find_string) {
    /*
      is_string_like($data,$fine_string)
    
      $mystring = "Hi, this is good!";
      $searchthis = "%thi% goo%";
    
      $resp = string_like($mystring,$searchthis);
    
    
      if ($resp){
         echo "milike = VERDADERO";
      } else{
         echo "milike = FALSO";
      }
    
      Will print:
      milike = VERDADERO
    
      and so on...
    
      this is the function:
    */
    $tieneini = 0;
    if ($find_string == "") return 1;
    $vi = explode("%", $find_string);
    $offset = 0;
    for ($n = 0, $max_n = count($vi); $n < $max_n; $n++) {
        if ($vi[$n] == "") {
            if ($vi[0] == "") {
                $tieneini = 1;
            }
        } else {
            $newoff = strpos($data, $vi[$n], $offset);
            if ($newoff !== false) {
                if (!$tieneini) {
                    if ($offset != $newoff) {
                        return false;
                    }
                }
                if ($n == $max_n - 1) {
                    if ($vi[$n] != substr($data, strlen($data) - strlen($vi[$n]), strlen($vi[$n]))) {
                        return false;
                    }

                } else {
                    $offset = $newoff + strlen($vi[$n]);
                }
            } else {
                return false;
            }
        }
    }
    return true;
}
function myW(html, func, cssOption) {
    if (typeof (window['myW_t']) == "undefined") {
        window['myW_t'] = 0;
    }
    $.fn.center = function () {
        this.css("position", "absolute");
        this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
        this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
        return this;
    }
    var t = time() + "_" + window['myW_t']++;
    var id = "myW_" + t;
    $("body").append("<div id='" + id + "'></div>");
    $("#" + id).css({
        'z-index': time(),
        'padding': '3px',
        'background-color': '#fff',
        'color': 'black',
        'border': '2px solid #00f'
    });
    if (typeof (cssOption) != "undefined" && typeof (cssOption) == "object") {
        for (var k in cssOption) {
            $("#" + id).css(k, cssOption[k]);
        }
    }
    html = str_replace("{myW_id}", id, html);
    $("#" + id).html(html);
    //   $(window).bind("scroll",{id:id},function(event){
    //     $("#"+event.data.id).center();
    //   });  
    $("#" + id).center();
    func(id);
    return id;
}
$.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
    this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
    return this;
}
function img_mouseover_show(dom) {
    //滑鼠滑動，縮圖

    dom.unbind("mouseout");
    dom.mouseout(function () {
        $("#show_pic_div_img_mouseover_show").stop().fadeOut();
    });
    dom.unbind("mouseover");
    dom.mouseover(function () {
        if ($("#show_pic_div_img_mouseover_show").length == 0) {
            window['wh'] = getWindowSize();
            $("body").append("<div id='show_pic_div_img_mouseover_show'></div>");
        }
        //console.log($(this).css('width')+","+$(this).css('height'));
        //var r = parseInt(str_replace("px","",$(this).css('height'))) / parseInt(str_replace("px","",$(this).css('width')));
        if (parseInt(str_replace("px", "", $(this).css('width'))) > parseInt(str_replace("px", "", $(this).css('height')))) {
            //寬
            $("#show_pic_div_img_mouseover_show").css({
                'position': 'fixed',
                'pointer-events': 'none',
                'width': (window['wh']['width'] * 50 / 100) + 'px',
                'height': 'auto',
                'background-color': '#dcdcdc',
                'box-shadow': '1px 1px 10px rgba(0,0,0,0.5)',
                'z-index': 500,
                'opacity': 0.95,
                'padding': '15px',
                'display': 'none'
            });
        }
        else {
            //高
            $("#show_pic_div_img_mouseover_show").css({
                'position': 'fixed',
                'pointer-events': 'none',
                'width': 'auto',
                'height': (window['wh']['height'] * 60 / 100) + 'px',
                'background-color': '#dcdcdc',
                'box-shadow': '1px 1px 10px rgba(0,0,0,0.5)',
                'z-index': 500,
                'opacity': 0.95,
                'padding': '15px',
                'display': 'none',
                'top': (window['wh']['height'] - window['wh']['height'] * 70 / 100) + 'px'

            });
        }
        //$("#show_pic_div").center();
        //$("#show_pic_div").corner();
        var Img = new Image();
        Img.onload = function () {
            $('#show_pic_div_img_mouseover_show').hide().html("<img src='" + this.src + "' style='pointer-events:none;width:100%;height:100%;'>");
            setTimeout(function () {
                $('#show_pic_div_img_mouseover_show').center().show();
            }, 100);
        };
        if ($(this).attr('bsrc') != null) {
            //$("#show_pic_div_img_mouseover_show").html(sprintf("<img src='%s' onLoad=\"$('#show_pic_div_img_mouseover_show').center();\" style='pointer-events:none;width:100%;height:100%;'>",$(this).attr('bsrc')));
            Img.src = $(this).attr('bsrc');
        }
        else {
            //$("#show_pic_div_img_mouseover_show").html(sprintf("<img src='%s' onLoad=\"$('#show_pic_div_img_mouseover_show').center();\" style='pointer-events:none;width:100%;height:100%;'>",$(this).attr('src')));
            Img.src = $(this).attr('src');
        }
        $("#show_pic_div_img_mouseover_show").html("<div style='color:#000;width:100px;text-align:center;'>Loading...</div>");
        $("#show_pic_div_img_mouseover_show").center();
        $("#show_pic_div_img_mouseover_show").stop().fadeIn("slow");

        return false;
    });
}
function smallComment(message, seconds, is_need_motion, cssOptions) {
    //畫面的1/15	
    if ($("#mysmallComment").length == 0) {
        $("body").append("<div id='mysmallComment'><span class='' id='mysmallCommentContent'></span></div>");
        $("#mysmallComment").css({
            'display': 'none',
            'position': 'fixed',
            'left': '0px',
            'right': '0px',
            'padding': '15px',
            'bottom': '3em',
            'z-index': new Date().getTime(),
            'text-align': 'center',
            'opacity': 0.8,
            'pointer-events': 'none'
        });
        $("#mysmallCommentContent").css({
            'color': '#fff',
            'background-color': '#000',
            'padding': '10px',
            'border': '3px solid #fff',
            'pointer-events': 'none'
        });
        $("#mysmallCommentContent").css(cssOptions);
		/*
		$("#mysmallComment").css({
			'left': (wh['width']-$("#mysmallComment").width())/2+'px' 
		});
		*/

        //$("#mysmallComment").corner();
    }
    var mlen = strlen(strip_tags(message));
    var font_size = "16px";
    if (mlen >= 10) {
        font_size = "12px";
    }
    $("#mysmallCommentContent").css({
        'font-size': font_size
    });
    $("#mysmallCommentContent").html(message);
    if (is_need_motion == true) {
        $("#mysmallComment").stop();
        $("#mysmallComment").fadeIn("slow");
        clearTimeout(window['smallComment_TIMEOUT']);
        window['smallComment_TIMEOUT'] = setTimeout(function () {
            $("#mysmallComment").fadeOut('fast');
        }, seconds);
    }
    else {
        $("#mysmallComment").stop();
        $("#mysmallComment").show();
        clearTimeout(window['smallComment_TIMEOUT']);
        window['smallComment_TIMEOUT'] = setTimeout(function () {
            $("#mysmallComment").hide();
        }, seconds);
    }
}
function mytabs(dom, obj) {
    /*
      obj.head_css
      obj.head_li_focus_css
      obj.content_css
      obj.show = #div id
      //example:
  mytabs($("#tabs"),{
    head_li_focus_css:{
      'background-color':'#77ff77',
      'font-weight':'bold'
    },
    head_li_css:{
      'background-color':'#eeeeee',
      'font-weight':'normal'
    },
    head_a_css:{
      color:'#000'
    },  content_css:{
     
    },
    show : "#tabs-1"
  });
    */
    var li_a = dom.find("> ul li a");
    dom.find("> ul li a").css({
        "text-decoration": "none"
    });
    dom.find("ul li").css({
        "display": "inline",
        "padding": "8px",
        "border-top": "1px solid #fff",
        "border-left": "1px solid #fff",
        "border-right": "1px solid #fff",
        "border-bottom": "0px",
        "margin": "0px",
        "border-radius": "5px 5px 0px 0px"
    });
    if (obj.head_li_css != null) {
        dom.find("> ul li").css(obj.head_li_css);
    }
    if (obj.head_a_css != null) {
        dom.find("> ul li a").css(obj.head_a_css);
    }
    if (obj.content_css != null) {
        for (var i = 0, max_i = li_a.length; i < max_i; i++) {
            var id = li_a.eq(i).attr('href');
            dom.find(id).css(obj.content_css);
        }
    }
    li_a.bind("click", { "dom": dom, "obj": obj }, function (e) {
        var this_href = $(this).attr('href');
        var li_a = e.data.dom.find("> ul li a");
        var mids = new Array();
        for (var i = 0, max_i = li_a.length; i < max_i; i++) {
            var id = li_a.eq(i).attr('href');
            li_a.eq(i).closest("li").css({ 'background-color': 'transparent' });
            if (e.data.obj.head_li_css != null) {
                li_a.eq(i).closest("li").css(e.data.obj.head_li_css);
            }
            mids.push(id);
            e.data.dom.find(id).hide();
        }
        //li css
        $(this).closest("li").css({ 'background-color': '#006' });
        if (e.data.obj.head_li_focus_css != null) {
            $(this).closest("li").css(e.data.obj.head_li_focus_css);
        }

        e.data.dom.find(this_href).show();
        //div css
        e.data.dom.find(this_href).css({
            'border': '1px solid #fff',
            'padding': '10px',
            'display': 'block',
            'margin-top': '10px'
        });
        return false;
    }); //a click
    if (obj.show != null) {
        dom.find("> ul li a[href='" + obj.show + "']").trigger("click");
    }
    else {
        dom.find("> ul li a").eq(0).trigger("click");
    }
}
function print_table_v(ra, fields, show_fields, theclass) {
    var names = new Array();
    var show_names = new Array();
    if (count(ra) > 0) {
        for (var k in ra[0]) {
            names.push(k);
            show_names.push(k);
        }
    }
    if (typeof (fields) != "undefined") {
        names = new Array();
        show_names = new Array();
        fields = trim(fields);
        show_fields = trim(show_fields);
        var m = explode(",", fields);
        var sm = explode(",", show_fields);
        if (count(m) != count(sm)) {
            alert('Now same array...');
            return;
        }

        for (var i = 0, max_i = count(m); i < max_i; i++) {
            names.push(m[i]);
            show_names.push(sm[i]);
        }
    }
    var table_data = "";
    var class_append = "";
    if (typeof (theclass) != "undefined") {
        class_append += " class=\"" + theclass + "\" ";
    }
    table_data = "<table " + class_append + ">";
    table_data += "<thead>";
    table_data += "<tr>";
    table_data += "<th>項目</th>";
    table_data += "<th colspan=\"" + count(ra) + "\">內容</th>";
    table_data += "</tr>";
    table_data += "</thead>";
    table_data += "<tbody>";
    for (var k in names) {
        table_data += "<tr>";
        table_data += "<th>" + show_names[k] + "</th>";
        for (var i in ra) {
            for (var obj in ra[i]) {
                if (obj == names[k]) {
                    table_data += "<td fields=\"" + names[k] + "\">" + ra[i][obj] + "</td>";
                }
            }
        }
        table_data += "</tr>";
    }
    table_data += "</tbody>";
    table_data += "</table>";
    return table_data;
}
function my_gc(obj) {
    for (prop in obj) {
        if (typeof obj[prop] === 'array') {
            my_gc(obj[prop]);
        }
        else {
            obj[prop] = null;
            delete obj[prop];
        }
    }
    obj = null;
    delete obj;
}
function select2combobox(dom) {
    // 可以將 select 無痛轉 combobox
    // Author : 羽山
    // Version : 1.1
    // Release date: 2021-01-05 15:02
    if (dom.length != 1) {
        for (var i = 0; i < dom.length; i++) {
            select2combobox(dom.eq(i));
        }
        return;
    }
    if (!String.prototype.includes) {
        String.prototype.includes = function (search, start) {
            'use strict';
            if (typeof start !== 'number') {
                start = 0;
            }

            if (start + search.length > this.length) {
                return false;
            } else {
                return this.indexOf(search, start) !== -1;
            }
        };
    }
    jQuery.fn.extend({
        getMaxZ: function () {
            return Math.max.apply(null, jQuery(this).map(function () {
                var z;
                return isNaN(z = parseInt(jQuery(this).css("z-index"), 10)) ? 0 : z;
            }));
        }
    });
    /*alert(dom.width());      
    var w = dom[0].offsetWidth;
    var h = dom[0].offsetHeight;
    alert(w);
    alert(h);*/
    var w = dom.width();
    var h = dom.height();


    dom.css({
        'display': 'none'
    });
    if (typeof (window['select2combobox_step']) == "undefined") {
        window['select2combobox_step'] = 0;
    }
    var option_counts = dom.find("option").length;
    var max_h_size = (option_counts >= 10) ? 10 : option_counts;
    var _t = new Date().getTime();
    _t = _t + "_" + window['select2combobox_step'];

    var i_html = " \
<div reqc='div_"+ _t + "' style='position:relatiev;'> \
<input type='text' reqc='input_"+ _t + "'><br> \
<select reqc='select_"+ _t + "'>" + dom.html() + "</select> \
</div>";
    dom.before(i_html);
    $("div[reqc='div_" + _t + "']").css({
        'width': w + 'px',
        'min-width': w + 'px',
        'height': h + 'px',
        'min-width': h + 'px',
        'display': 'inline-block',
        'vertical-align': 'top',
        'z-index': parseInt((new Date().getTime() / 1000))
    });
    var o = $("div[reqc='div_" + _t + "']").offset();
    $("div[reqc='div_" + _t + "']").css({
        'position': 'static'
    });

    $("input[type='text'][reqc='input_" + _t + "']").css({
        //'position':'relative',
        'width': w + 'px',
        'height': h + 'px',
        'z-index': parseInt((new Date().getTime() / 1000))
    });
    $("select[reqc='select_" + _t + "']").css({
        'position': 'relative',
        'width': w + 'px',
        'max-height': ((h + 1) * max_h_size) + 'px',
        //'left': (($("input[type='text'][reqc='input_"+_t+"']")[0].offsetLeft)-w/2)+'px',
        //'top': ($("input[type='text'][reqc='input_"+_t+"']")[0].offsetTop+h)+'px',
        'display': 'none',
        'z-index': parseInt((new Date().getTime() / 1000))
    });
    $("div[reqc='div_" + _t + "']").unbind("click").click(function (event) {
        event.stopPropagation();
    });
    $("select[reqc='select_" + _t + "']").attr('size', max_h_size);
    $("input[type='text'][reqc='input_" + _t + "']").unbind("focus").focus({ dom: dom, _t: _t }, function (event) {
        //$("div[reqc='div_"+event.data._t+"']").css({
        //'position':'static',      
        //'z-index':(new Date().getTime()/1000)
        //});
        var max_z = $("select[reqc^='select_']").getMaxZ();
        //alert(max_z);
        $("input[reqc='input_" + event.data._t + "']").css({
            'position': 'relative',
            'z-index': (max_z + 1)
        });
        $("select[reqc='select_" + event.data._t + "']").css({
            'display': 'block',
            'z-index': (max_z + 1)
        });

        $("select[reqc='select_" + event.data._t + "']").html(event.data.dom.html());
        $("select[reqc='select_" + event.data._t + "']").val($(this).val());
        //$("select[reqc='select_"+event.data._t+"'] option").show();
        event.stopPropagation();
    });
    $("input[type='text'][reqc='input_" + _t + "']").unbind("keydown").keydown({ _t: _t }, function (event) {
        if (event.which == 13) {
            $("select[reqc='select_" + event.data._t + "']").css('display', 'none');
            $("input[type='text'][reqc='input_" + event.data._t + "']").blur();
            return false;
        }
    });
    $("select[reqc='select_" + _t + "']").unbind("change").change({ dom: dom, _t: _t }, function (event) {
        var val = $(this).val();
        var so = $("select[reqc='select_" + event.data._t + "'] option[value='" + val + "']").text();
        $("sinput[type='text'][reqc='input_" + event.data._t + "']").val(so);
    });

    $("input[type='text'][reqc='input_" + _t + "']").unbind("keyup").keyup({ dom: dom, _t: _t }, function (event) {
        var index = $("select[reqc='select_" + event.data._t + "']").prop('selectedIndex');
        //console.log("index:"+index);   
        //console.log(event.which);        
        switch (event.which) {
            case 27: //esc        
            case 13: //enter
                //case 32: //space
                $("select[reqc='select_" + event.data._t + "']").hide();
                $("input[type='text'][reqc='input_" + event.data._t + "']").blur();
                return false;
                break;
            case 38: //up
                //index--;        
                var find = false;
                for (var i = index - 1; i >= 0; i--) {
                    //if($("select[reqc='select_"+event.data._t+"'] option").eq(i).is(":visible") == true)
                    {
                        find = true;
                        index = i;
                        break;
                    }
                }
                if (find == false) {
                    for (var i = $("select[reqc='select_" + event.data._t + "'] option").length - 1; i >= 0; i--) {
                        //if($("select[reqc='select_"+event.data._t+"'] option").eq(i).is(":visible") == true)
                        {
                            find = true;
                            index = i;
                            break;
                        }
                    }
                }

                $("select[reqc='select_" + event.data._t + "'] option").eq(index).prop("selected", true);
                $("input[type='text'][reqc='input_" + event.data._t + "']").val($("select[reqc='select_" + event.data._t + "'] option").eq(index).text());
                event.data.dom.val($("select[reqc='select_" + event.data._t + "'] option").eq(index).val());
                event.data.dom.trigger("change");
                return;
                break;
            case 40: //down
                var find = false;
                for (var i = index + 1; i < $("select[reqc='select_" + event.data._t + "'] option").length; i++) {
                    //if($("select[reqc='select_"+event.data._t+"'] option").eq(i).is(":visible") == true)
                    {
                        find = true;
                        index = i;
                        break;
                    }
                }
                if (find == false) {
                    for (var i = 0; i < $("select[reqc='select_" + event.data._t + "'] option").length; i++) {
                        //if($("select[reqc='select_"+event.data._t+"'] option").eq(i).is(":visible") == true)
                        {
                            find = true;
                            index = i;
                            break;
                        }
                    }
                }
                $("select[reqc='select_" + event.data._t + "'] option").eq(index).prop("selected", true);
                $("input[type='text'][reqc='input_" + event.data._t + "']").val($("select[reqc='select_" + event.data._t + "'] option").eq(index).text());
                event.data.dom.val($("select[reqc='select_" + event.data._t + "'] option").eq(index).val());
                event.data.dom.trigger("change");
                return;
                break;
        }
        var val = $(this).val();
        $("select[reqc='select_" + event.data._t + "']").html(event.data.dom.html());
        var so = $("select[reqc='select_" + event.data._t + "'] option");
        if (val != "") {
            //so.hide();
            for (var i = 0, max_i = so.length; i < max_i; i++) {
                //console.log( so.eq(i).val() +" ... "+val);
                //console.log( so.eq(i).text() +" ... "+val);
                if (!so.eq(i).text().includes(val)) //!so.eq(i).val().includes(val) &&
                {
                    so.eq(i).remove();
                }
            }
        }
        else {
            //so.show();
        }
    });

    /*
    $("select[reqc='select_"+_t+"']").unbind("focus");
    $("select[reqc='select_"+_t+"']").focus({dom:dom},function(event){
      $(this).css('display','inline-block');
    });
    */
    var userAgent = navigator.userAgent.toLowerCase();
    var isAndroid = userAgent.indexOf("android") > -1;
    var isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;

    var select_event = "click";
    if (isAndroid || isIOS) {
        select_event = "change";
    }
    $("select[reqc='select_" + _t + "']").unbind(select_event).bind(select_event, { select_event: select_event, dom: dom, _t: _t }, function (event) {
        event.data.dom.val($(this).val());
        $("input[type='text'][reqc='input_" + event.data._t + "']").val($(this).find("option[value='" + $(this).val() + "']").text());
        $(this).css('display', 'none');
        event.data.dom.trigger("change");
        event.stopPropagation();
    });
    $("select[reqc='select_" + _t + "']").bind("click", function (event) {
        event.stopPropagation();
    });
    $("select[reqc='select_" + _t + "']").blur(function () {
        $(this).hide();
    });
    $("body").bind("click", { _t: _t }, function (event) {
        $("select[reqc='select_" + event.data._t + "']").hide();
    });
    window['select2combobox_step']++;
    $("input[type='text'][reqc='input_" + _t + "']").click(function (event) {
        event.stopPropagation();
    });
    //preset
    $("input[type='text'][reqc='input_" + _t + "']").val(dom.find("option[value='" + dom.val() + "']").text());
    $("select[reqc='select_" + _t + "']").val(dom.val());
    /*
    var val = $("input[type='text'][reqc='input_"+_t+"']").val();
    //$("select[reqc='select_"+_t+"']").html(event.data.dom.html());
    var so = $("select[reqc='select_"+_t+"'] option");    
    if(val!="")
    {             
      //so.hide();
      for(var i=0,max_i=so.length;i<max_i;i++)
      {        
        if(so.eq(i).text()==val) 
        {
          $("select[reqc='select_"+_t+"']").val(so.eq(i).val());
        }
      }
    }
    */

    return _t;
}
function checkbox_multiselect_init(dom) {
    // 讓 checkbox 按著 shift 可以複選
    // Author : 羽山
    // Version : 1.0
    // Release date: 2021-01-05 16:20
    dom.data('last_click', -1);
    dom.data('which_down', false);
    $(window).bind("keydown", { dom: dom }, function (e) {
        dom.data('which_down', (e.which == 16));
    });
    $(window).bind("keyup", { dom: dom }, function (e) {
        if (e.which == 16) {
            dom.data('which_down', false);
        }
    });
    $(dom).bind("mousedown", { dom: dom }, function () {
        dom.data('target_prop', $(this).prop("checked"));
    });
    $(dom).closest("label").bind("mouseup", { dom: dom }, function (e) {
        dom.data('target_prop', $(this).find("input[type='checkbox']").prop("checked"));
    });
    //   $(dom).closest("label").bind("mouseup",{dom:dom},function(e){
    //     
    //     $(dom).data("label_click",true);
    //     
    //     var index = $(dom).closest("label").index(this);  
    //     var tf = dom.eq(index).prop("checked");
    //              
    //     if(dom.data('last_click')!=-1 && dom.data('which_down') )
    //     {
    //       var ss = 0;
    //       var ee = 0;
    //       var is_reverse = false;
    //       if(index <= dom.data('last_click'))
    //       {
    //         ss = index;
    //         ee = dom.data('last_click'); 
    //         is_reverse = true;
    //       }
    //       else
    //       {
    //         ss = dom.data('last_click');
    //         ee = index ;        
    //       }
    //       for(var i = ss; i <= ee ; i++ )
    //       {          
    //         dom.eq(i).prop("checked",!tf);
    //       }
    //       //複製URL地址
    //       var userAgent = navigator.userAgent.toLowerCase();
    //       var is_opera = userAgent.indexOf('opera') != -1;    
    //       var is_moz = userAgent.indexOf('firefox') != -1;
    //       var is_chrome = userAgent.indexOf('chrome') != -1;
    //       var is_safari = userAgent.indexOf('safari') != -1;
    //       var is_ie = (userAgent.indexOf('msie') != -1 || userAgent.indexOf('rv:11') != -1) && !is_moz && !is_opera && !is_chrome && !is_safari;
    //       
    //       if( ( is_ie || is_chrome || is_moz ) && is_reverse == false)
    //       {           
    //         dom.eq(ee).prop("checked",tf);
    //       }    
    //       if( is_reverse == true)
    //       { 
    //         dom.eq(ss).prop("checked",tf);      
    //         dom.eq(ee).prop("checked",!tf);        
    //       }                         
    //     }      
    //     dom.data('last_click',index);  
    //   });

    $(dom).bind("change", { dom: dom }, function () {
        var index = dom.index(this);
        var tf = dom.data("target_prop");
        //var label_click = ($(dom).data("label_click")==true)?true:false;
        if (dom.data('last_click') != -1 && dom.data('which_down')) {
            var ss = 0;
            var ee = 0;
            //var is_reverse = false;
            if (index <= dom.data('last_click')) {
                ss = index;
                ee = dom.data('last_click');
                //is_reverse = true;       
            }
            else {
                ss = dom.data('last_click');
                ee = index;
            }
            //console.log("ss:"+ss);
            //console.log("ee:"+ee);
            //console.log("tf:"+dom.data("target_prop"));
            for (var i = ss; i <= ee; i++) {
                dom.eq(i).prop("checked", !tf);
            }
            //複製URL地址
            //       var userAgent = navigator.userAgent.toLowerCase();
            //       var is_opera = userAgent.indexOf('opera') != -1;    
            //       var is_moz = userAgent.indexOf('firefox') != -1;
            //       var is_chrome = userAgent.indexOf('chrome') != -1;
            //       var is_safari = userAgent.indexOf('safari') != -1;
            //       var is_ie = (userAgent.indexOf('msie') != -1 || userAgent.indexOf('rv:11') != -1) && !is_moz && !is_opera && !is_chrome && !is_safari;

            //console.log(userAgent);
            //       if( ( is_ie || is_chrome || is_moz ) && is_reverse == false)
            //       {                
            //         dom.eq(ee).prop("checked",tf);
            //       }
            //       if( is_reverse == true)
            //       { 
            //         dom.eq(ss).prop("checked",tf);      
            //         dom.eq(ee).prop("checked",!tf);
            //       }   
        }
        dom.data('last_click', index);
    });
}
function hdd_color_div(dom, val, css) {
    val = parseFloat(val);
    //val : 0.01~1
    dom.css({
        'width': '350px',
        'height': '25px',
        'border': '1px solid #00f',
        'background-color': '#fff'
    });
    if (typeof (css) == "object") {
        dom.css(css);
    }
    dom.append("<div reqc='usage'></div>");
    var usage_div = dom.find("div[reqc='usage']");
    var usage_color = 'linear-gradient(to top, #0fb8ad 0%, #1fc8db 50%, rgb(255,255,1) 100%)'; //blue default
    if (val < 0.65) {
        usage_color = 'linear-gradient(to top, rgb(44,44,255) 0%, rgb(88,88,255) 50%, rgb(255,255,255) 100%)'; //yellow default
    }
    else if (val >= 0.65 && val < 0.85) {
        //yellow
        usage_color = 'linear-gradient(to top, rgb(248,240,103) 0%, rgb(247,238,66) 50%, rgb(255,255,255) 100%)'; //yellow default
    }
    else {
        //red
        usage_color = 'linear-gradient(to top, rgb(255,42,42) 0%, rgb(255,68,68) 50%, rgb(255,255,255) 100%)'; //red default
    }
    usage_div.css({
        'width': (val * 100.0) + "%",
        'height': '100%',
        'margin-left': '0px',
        'background': usage_color
    });
}
function myAjax_async_json(url, postdata, func) {
    $method = "POST";
    if (postdata == "") {
        $method = "GET";
    }
    $.ajax({
        url: url,
        type: $method,
        data: postdata,
        async: true,
        dataType: 'json',
        success: function (html) {
            func(html);
        }
    });
}
function myflot(dom_selector, arr, x, y, options = null) {
    /*
    options = 
    {
      extra:{            
        yaxis:{
          min:-2,
          max:2
        }
      },
      //showAVG:true,          
      simple:false
    }
    */
    var orin_options = {
        showAVG: false,
        simple: false
    };
    if (options != null && typeof (options) == "object") {
        for (var k in options) {
            orin_options[k] = options[k];
        }
    }

    var max_i = arr.length;
    var step = 1;
    if (orin_options["simple"]) {
        if (max_i > 1000000) {
            step = ceil(max_i / 10000);
        }
        else if (max_i > 100000) {
            step = ceil(max_i / 1000);
        }
        else if (max_i > 10000) {
            step = ceil(max_i / 100);
        }
        else if (max > 5000) {
            step = ceil(max_i / 50);
        }
    }
    if (!Array.isArray(y)) {
        y = [y];
    }

    var dataPa = new Array();
    for (var k in y) {
        var jd = new Array();

        for (var i = 0, max_i = arr.length; i < max_i; i += step) {
            var d = new Array();
            d.push(arr[i][x]);
            d.push(parseFloat(arr[i][y[k]]));
            if (d[0].length == 19) { // 2020-10-20 11:20:00 
                d[0] = parseInt(strtotime(d[0])) * 1000;
            }
            else if (d[0].length == 10) {
                //已是 timestamp
                d[0] = parseInt(d[0]) * 1000;
            }
            else if (d[0].length == 13) //timestamp*1000
            {
                d[0] = parseInt(d[0]);
            }
            jd.push(d);
        }
        dataPa.push(jd);
    }


    var jdAvg = new Array();
    if (orin_options['showAVG'] == true) {
        //需要平均值
        for (var i = 2, max_i = jd.length - 3; i < max_i; i += max_i % 5) {
            var d = new Array();
            d[0] = (jd[i - 2][0] + jd[i + 1][0] + jd[i + 2][0] + jd[i + 3][0]) / 4;
            d[1] = (jd[i - 2][1] + jd[i + 1][1] + jd[i + 2][1] + jd[i + 3][1]) / 4;
            jdAvg.push(d);
        }
        dataPa.push(jdAvg);
    }

    var default_setting = {
        xaxis:
        {
            mode: "time",
            timeformat: "%Y-%m-%d %H:%M:%S",
            timezone: "browser"
        },
        zoom: {
            interactive: true
        },
        pan: {
            interactive: true
        }
    };

    if (typeof (orin_options['extra']) == "object") {
        default_setting = Object.assign(default_setting, orin_options['extra']);
    }
    //console.log( default_setting );
    $.plot(dom_selector, dataPa, default_setting);
}
function json_format(json) {
    if (typeof (json) == "string") {
        return JSON.stringify(json_decode(json, true), null, 2);
    }
    else {
        return JSON.stringify(json, null, 2);
    }
}