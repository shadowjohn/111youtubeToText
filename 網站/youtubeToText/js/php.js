function my_fix_random() {
    //var array = new Uint32Array(1);
    //var v = window.crypto.getRandomValues(array)[0];
    //return parseFloat((v / (Math.pow(10, v.toString().length))).toFixed(10));    
    var crypto = window.crypto /*native*/ || window.msCrypto /*IE11 native*/ || window.msrCrypto; /*polyfill*/
    return parseFloat(((new Uint32Array(1))[0] / 4294967295).toString(36).substring(2, 15) + (crypto.getRandomValues(new Uint32Array(1))[0] / 4294967295));
}
function rand(min, max) {
    var argc = arguments.length; if (argc === 0) { min = 0; max = 2147483647; } else if (argc === 1) { throw new Error('Warning: rand() expects exactly 2 parameters, 1 given'); }
    return Math.floor(my_fix_random() * (max - min + 1)) + min;
}
function addslashes(str) { return (str + '').replace(/[\\"']/g, '\\$&').replace(/\u0000/g, '\\0'); }
function array_merge() {
    var args = Array.prototype.slice.call(arguments), retObj = {}, k, j = 0, i = 0, retArr = true; for (i = 0; i < args.length; i++) { if (!(args[i] instanceof Array)) { retArr = false; break; } }
    if (retArr) {
        retArr = []; for (i = 0; i < args.length; i++) { retArr = retArr.concat(args[i]); }
        return retArr;
    }
    var ct = 0; for (i = 0, ct = 0; i < args.length; i++) { if (args[i] instanceof Array) { for (j = 0; j < args[i].length; j++) { retObj[ct++] = args[i][j]; } } else { for (k in args[i]) { if (args[i].hasOwnProperty(k)) { if (parseInt(k, 10) + '' === k) { retObj[ct++] = args[i][k]; } else { retObj[k] = args[i][k]; } } } } }
    return retObj;
}
function array_values(input) {
    var tmp_arr = [], cnt = 0; var wtfkey = ''; for (wtfkey in input) { tmp_arr[cnt] = input[wtfkey]; cnt++; }
    return tmp_arr;
}
function array_push(inputArr) {
    var i = 0, pr = '', argv = arguments, argc = argv.length, allDigits = /^\d$/, size = 0, highestIdx = 0, len = 0; if (inputArr.hasOwnProperty('length')) {
        for (i = 1; i < argc; i++) { inputArr[inputArr.length] = argv[i]; }
        return inputArr.length;
    }
    for (pr in inputArr) { if (inputArr.hasOwnProperty(pr)) { ++len; if (pr.search(allDigits) !== -1) { size = parseInt(pr, 10); highestIdx = size > highestIdx ? size : highestIdx; } } }
    for (i = 1; i < argc; i++) { inputArr[++highestIdx] = argv[i]; }
    return len + i - 1;
}
function arsort(inputArr, sort_flags) {
    var valArr = [], keyArr = [], k, i, ret, sorter, that = this, strictForIn = false, populateArr = {}; switch (sort_flags) {
        case 'SORT_STRING': sorter = function (a, b) { return that.strnatcmp(b, a); }; break; case 'SORT_LOCALE_STRING': var loc = this.i18n_loc_get_default(); sorter = this.php_js.i18nLocales[loc].sorting; break; case 'SORT_NUMERIC': sorter = function (a, b) { return (a - b); }; break; case 'SORT_REGULAR': default: sorter = function (a, b) {
            if (a > b) { return 1; }
            if (a < b) { return -1; }
            return 0;
        }; break;
    }
    var bubbleSort = function (keyArr, inputArr) { var i, j, tempValue, tempKeyVal; for (i = inputArr.length - 2; i >= 0; i--) { for (j = 0; j <= i; j++) { ret = sorter(inputArr[j + 1], inputArr[j]); if (ret > 0) { tempValue = inputArr[j]; inputArr[j] = inputArr[j + 1]; inputArr[j + 1] = tempValue; tempKeyVal = keyArr[j]; keyArr[j] = keyArr[j + 1]; keyArr[j + 1] = tempKeyVal; } } } }; this.php_js = this.php_js || {}; this.php_js.ini = this.php_js.ini || {}; strictForIn = this.php_js.ini['phpjs.strictForIn'] && this.php_js.ini['phpjs.strictForIn'].local_value && this.php_js.ini['phpjs.strictForIn'].local_value !== 'off'; populateArr = strictForIn ? inputArr : populateArr; for (k in inputArr) { if (inputArr.hasOwnProperty(k)) { valArr.push(inputArr[k]); keyArr.push(k); if (strictForIn) { delete inputArr[k]; } } }
    try { bubbleSort(keyArr, valArr); } catch (e) { return false; }
    for (i = 0; i < valArr.length; i++) { populateArr[keyArr[i]] = valArr[i]; }
    return strictForIn || populateArr;
}
function asort(inputArr, sort_flags) {
    var valArr = [], keyArr = [], k, i, ret, sorter, that = this, strictForIn = false, populateArr = {}; switch (sort_flags) {
        case 'SORT_STRING': sorter = function (a, b) { return that.strnatcmp(a, b); }; break; case 'SORT_LOCALE_STRING': var loc = this.i18n_loc_get_default(); sorter = this.php_js.i18nLocales[loc].sorting; break; case 'SORT_NUMERIC': sorter = function (a, b) { return (a - b); }; break; case 'SORT_REGULAR': default: sorter = function (a, b) {
            if (a > b) { return 1; }
            if (a < b) { return -1; }
            return 0;
        }; break;
    }
    var bubbleSort = function (keyArr, inputArr) { var i, j, tempValue, tempKeyVal; for (i = inputArr.length - 2; i >= 0; i--) { for (j = 0; j <= i; j++) { ret = sorter(inputArr[j + 1], inputArr[j]); if (ret < 0) { tempValue = inputArr[j]; inputArr[j] = inputArr[j + 1]; inputArr[j + 1] = tempValue; tempKeyVal = keyArr[j]; keyArr[j] = keyArr[j + 1]; keyArr[j + 1] = tempKeyVal; } } } }; this.php_js = this.php_js || {}; this.php_js.ini = this.php_js.ini || {}; strictForIn = this.php_js.ini['phpjs.strictForIn'] && this.php_js.ini['phpjs.strictForIn'].local_value && this.php_js.ini['phpjs.strictForIn'].local_value !== 'off'; populateArr = strictForIn ? inputArr : populateArr; for (k in inputArr) { if (inputArr.hasOwnProperty(k)) { valArr.push(inputArr[k]); keyArr.push(k); if (strictForIn) { delete inputArr[k]; } } }
    try { bubbleSort(keyArr, valArr); } catch (e) { return false; }
    for (i = 0; i < valArr.length; i++) { populateArr[keyArr[i]] = valArr[i]; }
    return strictForIn || populateArr;
}
function base64_decode(data) {
    var b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="; var o1, o2, o3, h1, h2, h3, h4, bits, i = 0, ac = 0, dec = "", tmp_arr = []; if (!data) { return data; }
    data += ''; do { h1 = b64.indexOf(data.charAt(i++)); h2 = b64.indexOf(data.charAt(i++)); h3 = b64.indexOf(data.charAt(i++)); h4 = b64.indexOf(data.charAt(i++)); bits = h1 << 18 | h2 << 12 | h3 << 6 | h4; o1 = bits >> 16 & 0xff; o2 = bits >> 8 & 0xff; o3 = bits & 0xff; if (h3 == 64) { tmp_arr[ac++] = String.fromCharCode(o1); } else if (h4 == 64) { tmp_arr[ac++] = String.fromCharCode(o1, o2); } else { tmp_arr[ac++] = String.fromCharCode(o1, o2, o3); } } while (i < data.length); dec = tmp_arr.join(''); dec = this.utf8_decode(dec); return dec;
}
function base64_encode(data) {
    var b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="; var o1, o2, o3, h1, h2, h3, h4, bits, i = 0, ac = 0, enc = "", tmp_arr = []; if (!data) { return data; }
    data = this.utf8_encode(data + ''); do { o1 = data.charCodeAt(i++); o2 = data.charCodeAt(i++); o3 = data.charCodeAt(i++); bits = o1 << 16 | o2 << 8 | o3; h1 = bits >> 18 & 0x3f; h2 = bits >> 12 & 0x3f; h3 = bits >> 6 & 0x3f; h4 = bits & 0x3f; tmp_arr[ac++] = b64.charAt(h1) + b64.charAt(h2) + b64.charAt(h3) + b64.charAt(h4); } while (i < data.length); enc = tmp_arr.join(''); switch (data.length % 3) { case 1: enc = enc.slice(0, -2) + '=='; break; case 2: enc = enc.slice(0, -1) + '='; break; }
    return enc;
}
function count(mixed_var, mode) {
    var key, cnt = 0; if (mixed_var === null) { return 0; } else if (mixed_var.constructor !== Array && mixed_var.constructor !== Object) { return 1; }
    if (mode === 'COUNT_RECURSIVE') { mode = 1; }
    if (mode != 1) { mode = 0; }
    for (key in mixed_var) { if (mixed_var.hasOwnProperty(key)) { cnt++; if (mode == 1 && mixed_var[key] && (mixed_var[key].constructor === Array || mixed_var[key].constructor === Object)) { cnt += this.count(mixed_var[key], 1); } } }
    return cnt;
}
function date(format, timestamp) { var that = this, jsdate, f, formatChr = /\\?([a-z])/gi, formatChrCb, _pad = function (n, c) { if ((n = n + "").length < c) { return new Array((++c) - n.length).join("0") + n; } else { return n; } }, txt_words = ["Sun", "Mon", "Tues", "Wednes", "Thurs", "Fri", "Satur", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"], txt_ordin = { 1: "st", 2: "nd", 3: "rd", 21: "st", 22: "nd", 23: "rd", 31: "st" }; formatChrCb = function (t, s) { return f[t] ? f[t]() : s; }; f = { d: function () { return _pad(f.j(), 2); }, D: function () { return f.l().slice(0, 3); }, j: function () { return jsdate.getDate(); }, l: function () { return txt_words[f.w()] + 'day'; }, N: function () { return f.w() || 7; }, S: function () { return txt_ordin[f.j()] || 'th'; }, w: function () { return jsdate.getDay(); }, z: function () { var a = new Date(f.Y(), f.n() - 1, f.j()), b = new Date(f.Y(), 0, 1); return Math.round((a - b) / 864e5) + 1; }, W: function () { var a = new Date(f.Y(), f.n() - 1, f.j() - f.N() + 3), b = new Date(a.getFullYear(), 0, 4); return 1 + Math.round((a - b) / 864e5 / 7); }, F: function () { return txt_words[6 + f.n()]; }, m: function () { return _pad(f.n(), 2); }, M: function () { return f.F().slice(0, 3); }, n: function () { return jsdate.getMonth() + 1; }, t: function () { return (new Date(f.Y(), f.n(), 0)).getDate(); }, L: function () { var y = f.Y(), a = y & 3, b = y % 4e2, c = y % 1e2; return 0 + (!a && (c || !b)); }, o: function () { var n = f.n(), W = f.W(), Y = f.Y(); return Y + (n === 12 && W < 9 ? -1 : n === 1 && W > 9); }, Y: function () { return jsdate.getFullYear(); }, y: function () { return (f.Y() + "").slice(-2); }, a: function () { return jsdate.getHours() > 11 ? "pm" : "am"; }, A: function () { return f.a().toUpperCase(); }, B: function () { var H = jsdate.getUTCHours() * 36e2, i = jsdate.getUTCMinutes() * 60, s = jsdate.getUTCSeconds(); return _pad(Math.floor((H + i + s + 36e2) / 86.4) % 1e3, 3); }, g: function () { return f.G() % 12 || 12; }, G: function () { return jsdate.getHours(); }, h: function () { return _pad(f.g(), 2); }, H: function () { return _pad(f.G(), 2); }, i: function () { return _pad(jsdate.getMinutes(), 2); }, s: function () { return _pad(jsdate.getSeconds(), 2); }, u: function () { return _pad(jsdate.getMilliseconds() * 1000, 6); }, e: function () { return 'UTC'; }, I: function () { var a = new Date(f.Y(), 0), c = Date.UTC(f.Y(), 0), b = new Date(f.Y(), 6), d = Date.UTC(f.Y(), 6); return 0 + ((a - c) !== (b - d)); }, O: function () { var a = jsdate.getTimezoneOffset(); return (a > 0 ? "-" : "+") + _pad(Math.abs(a / 60 * 100), 4); }, P: function () { var O = f.O(); return (O.substr(0, 3) + ":" + O.substr(3, 2)); }, T: function () { return 'UTC'; }, Z: function () { return -jsdate.getTimezoneOffset() * 60; }, c: function () { return 'Y-m-d\\Th:i:sP'.replace(formatChr, formatChrCb); }, r: function () { return 'D, d M Y H:i:s O'.replace(formatChr, formatChrCb); }, U: function () { return jsdate.getTime() / 1000 | 0; } }; this.date = function (format, timestamp) { that = this; jsdate = ((typeof timestamp === 'undefined') ? new Date() : (timestamp instanceof Date) ? new Date(timestamp) : new Date(timestamp * 1000)); return format.replace(formatChr, formatChrCb); }; return this.date(format, timestamp); }
function end(arr) {
    this.php_js = this.php_js || {}; this.php_js.pointers = this.php_js.pointers || []; var indexOf = function (value) {
        for (var i = 0, length = this.length; i < length; i++) { if (this[i] === value) { return i; } }
        return -1;
    }; var pointers = this.php_js.pointers; if (!pointers.indexOf) { pointers.indexOf = indexOf; }
    if (pointers.indexOf(arr) === -1) { pointers.push(arr, 0); }
    var arrpos = pointers.indexOf(arr); if (!(arr instanceof Array)) {
        var ct = 0; for (var k in arr) { ct++; var val = arr[k]; }
        if (ct === 0) { return false; }
        pointers[arrpos + 1] = ct - 1; return val;
    }
    if (arr.length === 0) { return false; }
    pointers[arrpos + 1] = arr.length - 1; return arr[pointers[arrpos + 1]];
}
function explode(delimiter, string, limit) {
    var emptyArray = { 0: '' }; if (arguments.length < 2 || typeof arguments[0] == 'undefined' || typeof arguments[1] == 'undefined') { return null; }
    if (delimiter === '' || delimiter === false || delimiter === null) { return false; }
    if (typeof delimiter == 'function' || typeof delimiter == 'object' || typeof string == 'function' || typeof string == 'object') { return emptyArray; }
    if (delimiter === true) { delimiter = '1'; }
    if (!limit) { return string.toString().split(delimiter.toString()); } else { var splitted = string.toString().split(delimiter.toString()); var partA = splitted.splice(0, limit - 1); var partB = splitted.join(delimiter.toString()); partA.push(partB); return partA; }
}
function htmlentities(string, quote_style) {
    var hash_map = {}, symbol = '', tmp_str = '', entity = ''; tmp_str = string.toString(); if (false === (hash_map = this.get_html_translation_table('HTML_ENTITIES', quote_style))) { return false; }
    hash_map["'"] = '&#039;'; for (symbol in hash_map) { entity = hash_map[symbol]; tmp_str = tmp_str.split(symbol).join(entity); }
    return tmp_str;
}
function htmlspecialchars(string, quote_style, charset, double_encode) {
    var optTemp = 0, i = 0, noquotes = false; if (typeof quote_style === 'undefined' || quote_style === null) { quote_style = 2; }
    string = string.toString(); if (double_encode !== false) { string = string.replace(/&/g, '&amp;'); }
    string = string.replace(/</g, '&lt;').replace(/>/g, '&gt;'); var OPTS = { 'ENT_NOQUOTES': 0, 'ENT_HTML_QUOTE_SINGLE': 1, 'ENT_HTML_QUOTE_DOUBLE': 2, 'ENT_COMPAT': 2, 'ENT_QUOTES': 3, 'ENT_IGNORE': 4 }; if (quote_style === 0) { noquotes = true; }
    if (typeof quote_style !== 'number') {
        quote_style = [].concat(quote_style); for (i = 0; i < quote_style.length; i++) {
            if (OPTS[quote_style[i]] === 0) { noquotes = true; }
            else if (OPTS[quote_style[i]]) { optTemp = optTemp | OPTS[quote_style[i]]; }
        }
        quote_style = optTemp;
    }
    if (quote_style & OPTS.ENT_HTML_QUOTE_SINGLE) { string = string.replace(/'/g, '&#039;'); }
    if (!noquotes) { string = string.replace(/"/g, '&quot;'); }
    return string;
}
function htmlspecialchars_decode(string, quote_style) {
    var optTemp = 0, i = 0, noquotes = false; if (typeof quote_style === 'undefined') { quote_style = 2; }
    string = string.toString().replace(/&lt;/g, '<').replace(/&gt;/g, '>'); var OPTS = { 'ENT_NOQUOTES': 0, 'ENT_HTML_QUOTE_SINGLE': 1, 'ENT_HTML_QUOTE_DOUBLE': 2, 'ENT_COMPAT': 2, 'ENT_QUOTES': 3, 'ENT_IGNORE': 4 }; if (quote_style === 0) { noquotes = true; }
    if (typeof quote_style !== 'number') {
        quote_style = [].concat(quote_style); for (i = 0; i < quote_style.length; i++) {
            if (OPTS[quote_style[i]] === 0) { noquotes = true; }
            else if (OPTS[quote_style[i]]) { optTemp = optTemp | OPTS[quote_style[i]]; }
        }
        quote_style = optTemp;
    }
    if (quote_style & OPTS.ENT_HTML_QUOTE_SINGLE) { string = string.replace(/&#0*39;/g, "'"); }
    if (!noquotes) { string = string.replace(/&quot;/g, '"'); }
    string = string.replace(/&amp;/g, '&'); return string;
}
function http_build_query(formdata, numeric_prefix, arg_separator) {
    var value, key, tmp = []; var _http_build_query_helper = function (key, val, arg_separator) {
        var k, tmp = []; if (val === true) { val = "1"; } else if (val === false) { val = "0"; }
        if (val !== null && typeof (val) === "object") {
            for (k in val) { if (val[k] !== null) { tmp.push(_http_build_query_helper(key + "[" + k + "]", val[k], arg_separator)); } }
            return tmp.join(arg_separator);
        } else if (typeof (val) !== "function") { return this.urlencode(key) + "=" + this.urlencode(val); } else { throw new Error('There was an error processing for http_build_query().'); }
    }; if (!arg_separator) { arg_separator = "&"; }
    for (key in formdata) {
        value = formdata[key]; if (numeric_prefix && !isNaN(key)) { key = String(numeric_prefix) + key; }
        tmp.push(_http_build_query_helper(key, value, arg_separator));
    }
    return tmp.join(arg_separator);
}
function implode(glue, pieces) {
    var i = '', retVal = '', tGlue = ''; if (arguments.length === 1) { pieces = glue; glue = ''; }
    if (typeof (pieces) === 'object') {
        if (pieces instanceof Array) { return pieces.join(glue); }
        else {
            for (i in pieces) { retVal += tGlue + pieces[i]; tGlue = glue; }
            return retVal;
        }
    }
    else { return pieces; }
}
function in_array(needle, haystack, argStrict) {
    var wtfkey = '', strict = !!argStrict; if (strict) { for (wtfkey in haystack) { if (haystack[wtfkey] === needle) { return true; } } } else { for (wtfkey in haystack) { if (haystack[wtfkey] == needle) { return true; } } }
    return false;
}
function is_numeric(mixed_var) { return (typeof (mixed_var) === 'number' || typeof (mixed_var) === 'string') && mixed_var !== '' && !isNaN(mixed_var); }
function json_decode(sjson) {
    var json = this.window.JSON; if (typeof json === 'object' && typeof json.parse === 'function') {
        try { return json.parse(sjson); } catch (err) {
            if (!(err instanceof SyntaxError)) { throw new Error('Unexpected error type in json_decode()'); }
            this.php_js = this.php_js || {}; this.php_js.last_error_json = 4; return null;
        }
    }
    var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g; var j; var text = str_json; cx.lastIndex = 0; if (cx.test(text)) {
        text = text.replace(cx, function (a) {
            return '\\u' +
                ('0000' + a.charCodeAt(0).toString(16)).slice(-4);
        });
    }
    if ((/^[\],:{}\s]*$/).test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, '@').replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, ']').replace(/(?:^|:|,)(?:\s*\[)+/g, ''))) { j = eval('(' + text + ')'); return j; }
    this.php_js = this.php_js || {}; this.php_js.last_error_json = 4; return null;
}
function json_encode(mixed_val) {
    var retVal, json = this.window.JSON; try {
        if (typeof json === 'object' && typeof json.stringify === 'function') {
            retVal = json.stringify(mixed_val); if (retVal === undefined) { throw new SyntaxError('json_encode'); }
            return retVal;
        }
        var value = mixed_val; var quote = function (string) { var escapable = /[\\\"\u0000-\u001f\u007f-\u009f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g; var meta = { '\b': '\\b', '\t': '\\t', '\n': '\\n', '\f': '\\f', '\r': '\\r', '"': '\\"', '\\': '\\\\' }; escapable.lastIndex = 0; return escapable.test(string) ? '"' + string.replace(escapable, function (a) { var c = meta[a]; return typeof c === 'string' ? c : '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4); }) + '"' : '"' + string + '"'; }; var str = function (wtfkey, holder) {
            var gap = ''; var indent = '    '; var i = 0; var k = ''; var v = ''; var length = 0; var mind = gap; var partial = []; var value = holder[wtfkey]; if (value && typeof value === 'object' && typeof value.toJSON === 'function') { value = value.toJSON(wtfkey); }
            switch (typeof value) {
                case 'string': return quote(value); case 'number': return isFinite(value) ? String(value) : 'null'; case 'boolean': case 'null': return String(value); case 'object': if (!value) { return 'null'; }
                    if ((this.PHPJS_Resource && value instanceof this.PHPJS_Resource) || (window.PHPJS_Resource && value instanceof window.PHPJS_Resource)) { throw new SyntaxError('json_encode'); }
                    gap += indent; partial = []; if (Object.prototype.toString.apply(value) === '[object Array]') {
                        length = value.length; for (i = 0; i < length; i += 1) { partial[i] = str(i, value) || 'null'; }
                        v = partial.length === 0 ? '[]' : gap ? '[\n' + gap +
                            partial.join(',\n' + gap) + '\n' +
                            mind + ']' : '[' + partial.join(',') + ']'; gap = mind; return v;
                    }
                    for (k in value) { if (Object.hasOwnProperty.call(value, k)) { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ': ' : ':') + v); } } }
                    v = partial.length === 0 ? '{}' : gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' +
                        mind + '}' : '{' + partial.join(',') + '}'; gap = mind; return v; case 'undefined': case 'function': default: throw new SyntaxError('json_encode');
            }
        }; return str('', { '': value });
    } catch (err) {
        if (!(err instanceof SyntaxError)) { throw new Error('Unexpected error type in json_encode()'); }
        this.php_js = this.php_js || {}; this.php_js.last_error_json = 4; return null;
    }
}
function max() {
    var ar, retVal, i = 0, n = 0; var argv = arguments, argc = argv.length; var _obj2Array = function (obj) {
        if (obj instanceof Array) { return obj; } else {
            var ar = []; for (var i in obj) { ar.push(obj[i]); }
            return ar;
        }
    }; var _compare = function (current, next) {
        var i = 0, n = 0, tmp = 0; var nl = 0, cl = 0; if (current === next) { return 0; } else if (typeof current == 'object') {
            if (typeof next == 'object') {
                current = _obj2Array(current); next = _obj2Array(next); cl = current.length; nl = next.length; if (nl > cl) { return 1; } else if (nl < cl) { return -1; } else {
                    for (i = 0, n = cl; i < n; ++i) { tmp = _compare(current[i], next[i]); if (tmp == 1) { return 1; } else if (tmp == -1) { return -1; } }
                    return 0;
                }
            } else { return -1; }
        } else if (typeof next == 'object') { return 1; } else if (isNaN(next) && !isNaN(current)) { if (current == 0) { return 0; } else { return (current < 0 ? 1 : -1); } } else if (isNaN(current) && !isNaN(next)) { if (next == 0) { return 0; } else { return (next > 0 ? 1 : -1); } } else { if (next == current) { return 0; } else { return (next > current ? 1 : -1); } }
    }; if (argc === 0) { throw new Error('At least one value should be passed to max()'); } else if (argc === 1) {
        if (typeof argv[0] === 'object') { ar = _obj2Array(argv[0]); } else { throw new Error('Wrong parameter count for max()'); }
        if (ar.length === 0) { throw new Error('Array must contain at least one element for max()'); }
    } else { ar = argv; }
    retVal = ar[0]; for (i = 1, n = ar.length; i < n; ++i) { if (_compare(retVal, ar[i]) == 1) { retVal = ar[i]; } }
    return retVal;
}
function md5(str) {
    var xl; var rotateLeft = function (lValue, iShiftBits) { return (lValue << iShiftBits) | (lValue >>> (32 - iShiftBits)); }; var addUnsigned = function (lX, lY) {
        var lX4, lY4, lX8, lY8, lResult; lX8 = (lX & 0x80000000); lY8 = (lY & 0x80000000); lX4 = (lX & 0x40000000); lY4 = (lY & 0x40000000); lResult = (lX & 0x3FFFFFFF) + (lY & 0x3FFFFFFF); if (lX4 & lY4) { return (lResult ^ 0x80000000 ^ lX8 ^ lY8); }
        if (lX4 | lY4) { if (lResult & 0x40000000) { return (lResult ^ 0xC0000000 ^ lX8 ^ lY8); } else { return (lResult ^ 0x40000000 ^ lX8 ^ lY8); } } else { return (lResult ^ lX8 ^ lY8); }
    }; var _F = function (x, y, z) { return (x & y) | ((~x) & z); }; var _G = function (x, y, z) { return (x & z) | (y & (~z)); }; var _H = function (x, y, z) { return (x ^ y ^ z); }; var _I = function (x, y, z) { return (y ^ (x | (~z))); }; var _FF = function (a, b, c, d, x, s, ac) { a = addUnsigned(a, addUnsigned(addUnsigned(_F(b, c, d), x), ac)); return addUnsigned(rotateLeft(a, s), b); }; var _GG = function (a, b, c, d, x, s, ac) { a = addUnsigned(a, addUnsigned(addUnsigned(_G(b, c, d), x), ac)); return addUnsigned(rotateLeft(a, s), b); }; var _HH = function (a, b, c, d, x, s, ac) { a = addUnsigned(a, addUnsigned(addUnsigned(_H(b, c, d), x), ac)); return addUnsigned(rotateLeft(a, s), b); }; var _II = function (a, b, c, d, x, s, ac) { a = addUnsigned(a, addUnsigned(addUnsigned(_I(b, c, d), x), ac)); return addUnsigned(rotateLeft(a, s), b); }; var convertToWordArray = function (str) {
        var lWordCount; var lMessageLength = str.length; var lNumberOfWords_temp1 = lMessageLength + 8; var lNumberOfWords_temp2 = (lNumberOfWords_temp1 - (lNumberOfWords_temp1 % 64)) / 64; var lNumberOfWords = (lNumberOfWords_temp2 + 1) * 16; var lWordArray = new Array(lNumberOfWords - 1); var lBytePosition = 0; var lByteCount = 0; while (lByteCount < lMessageLength) { lWordCount = (lByteCount - (lByteCount % 4)) / 4; lBytePosition = (lByteCount % 4) * 8; lWordArray[lWordCount] = (lWordArray[lWordCount] | (str.charCodeAt(lByteCount) << lBytePosition)); lByteCount++; }
        lWordCount = (lByteCount - (lByteCount % 4)) / 4; lBytePosition = (lByteCount % 4) * 8; lWordArray[lWordCount] = lWordArray[lWordCount] | (0x80 << lBytePosition); lWordArray[lNumberOfWords - 2] = lMessageLength << 3; lWordArray[lNumberOfWords - 1] = lMessageLength >>> 29; return lWordArray;
    }; var wordToHex = function (lValue) {
        var wordToHexValue = "", wordToHexValue_temp = "", lByte, lCount; for (lCount = 0; lCount <= 3; lCount++) { lByte = (lValue >>> (lCount * 8)) & 255; wordToHexValue_temp = "0" + lByte.toString(16); wordToHexValue = wordToHexValue + wordToHexValue_temp.substr(wordToHexValue_temp.length - 2, 2); }
        return wordToHexValue;
    }; var x = [], k, AA, BB, CC, DD, a, b, c, d, S11 = 7, S12 = 12, S13 = 17, S14 = 22, S21 = 5, S22 = 9, S23 = 14, S24 = 20, S31 = 4, S32 = 11, S33 = 16, S34 = 23, S41 = 6, S42 = 10, S43 = 15, S44 = 21; str = this.utf8_encode(str); x = convertToWordArray(str); a = 0x67452301; b = 0xEFCDAB89; c = 0x98BADCFE; d = 0x10325476; xl = x.length; for (k = 0; k < xl; k += 16) { AA = a; BB = b; CC = c; DD = d; a = _FF(a, b, c, d, x[k + 0], S11, 0xD76AA478); d = _FF(d, a, b, c, x[k + 1], S12, 0xE8C7B756); c = _FF(c, d, a, b, x[k + 2], S13, 0x242070DB); b = _FF(b, c, d, a, x[k + 3], S14, 0xC1BDCEEE); a = _FF(a, b, c, d, x[k + 4], S11, 0xF57C0FAF); d = _FF(d, a, b, c, x[k + 5], S12, 0x4787C62A); c = _FF(c, d, a, b, x[k + 6], S13, 0xA8304613); b = _FF(b, c, d, a, x[k + 7], S14, 0xFD469501); a = _FF(a, b, c, d, x[k + 8], S11, 0x698098D8); d = _FF(d, a, b, c, x[k + 9], S12, 0x8B44F7AF); c = _FF(c, d, a, b, x[k + 10], S13, 0xFFFF5BB1); b = _FF(b, c, d, a, x[k + 11], S14, 0x895CD7BE); a = _FF(a, b, c, d, x[k + 12], S11, 0x6B901122); d = _FF(d, a, b, c, x[k + 13], S12, 0xFD987193); c = _FF(c, d, a, b, x[k + 14], S13, 0xA679438E); b = _FF(b, c, d, a, x[k + 15], S14, 0x49B40821); a = _GG(a, b, c, d, x[k + 1], S21, 0xF61E2562); d = _GG(d, a, b, c, x[k + 6], S22, 0xC040B340); c = _GG(c, d, a, b, x[k + 11], S23, 0x265E5A51); b = _GG(b, c, d, a, x[k + 0], S24, 0xE9B6C7AA); a = _GG(a, b, c, d, x[k + 5], S21, 0xD62F105D); d = _GG(d, a, b, c, x[k + 10], S22, 0x2441453); c = _GG(c, d, a, b, x[k + 15], S23, 0xD8A1E681); b = _GG(b, c, d, a, x[k + 4], S24, 0xE7D3FBC8); a = _GG(a, b, c, d, x[k + 9], S21, 0x21E1CDE6); d = _GG(d, a, b, c, x[k + 14], S22, 0xC33707D6); c = _GG(c, d, a, b, x[k + 3], S23, 0xF4D50D87); b = _GG(b, c, d, a, x[k + 8], S24, 0x455A14ED); a = _GG(a, b, c, d, x[k + 13], S21, 0xA9E3E905); d = _GG(d, a, b, c, x[k + 2], S22, 0xFCEFA3F8); c = _GG(c, d, a, b, x[k + 7], S23, 0x676F02D9); b = _GG(b, c, d, a, x[k + 12], S24, 0x8D2A4C8A); a = _HH(a, b, c, d, x[k + 5], S31, 0xFFFA3942); d = _HH(d, a, b, c, x[k + 8], S32, 0x8771F681); c = _HH(c, d, a, b, x[k + 11], S33, 0x6D9D6122); b = _HH(b, c, d, a, x[k + 14], S34, 0xFDE5380C); a = _HH(a, b, c, d, x[k + 1], S31, 0xA4BEEA44); d = _HH(d, a, b, c, x[k + 4], S32, 0x4BDECFA9); c = _HH(c, d, a, b, x[k + 7], S33, 0xF6BB4B60); b = _HH(b, c, d, a, x[k + 10], S34, 0xBEBFBC70); a = _HH(a, b, c, d, x[k + 13], S31, 0x289B7EC6); d = _HH(d, a, b, c, x[k + 0], S32, 0xEAA127FA); c = _HH(c, d, a, b, x[k + 3], S33, 0xD4EF3085); b = _HH(b, c, d, a, x[k + 6], S34, 0x4881D05); a = _HH(a, b, c, d, x[k + 9], S31, 0xD9D4D039); d = _HH(d, a, b, c, x[k + 12], S32, 0xE6DB99E5); c = _HH(c, d, a, b, x[k + 15], S33, 0x1FA27CF8); b = _HH(b, c, d, a, x[k + 2], S34, 0xC4AC5665); a = _II(a, b, c, d, x[k + 0], S41, 0xF4292244); d = _II(d, a, b, c, x[k + 7], S42, 0x432AFF97); c = _II(c, d, a, b, x[k + 14], S43, 0xAB9423A7); b = _II(b, c, d, a, x[k + 5], S44, 0xFC93A039); a = _II(a, b, c, d, x[k + 12], S41, 0x655B59C3); d = _II(d, a, b, c, x[k + 3], S42, 0x8F0CCC92); c = _II(c, d, a, b, x[k + 10], S43, 0xFFEFF47D); b = _II(b, c, d, a, x[k + 1], S44, 0x85845DD1); a = _II(a, b, c, d, x[k + 8], S41, 0x6FA87E4F); d = _II(d, a, b, c, x[k + 15], S42, 0xFE2CE6E0); c = _II(c, d, a, b, x[k + 6], S43, 0xA3014314); b = _II(b, c, d, a, x[k + 13], S44, 0x4E0811A1); a = _II(a, b, c, d, x[k + 4], S41, 0xF7537E82); d = _II(d, a, b, c, x[k + 11], S42, 0xBD3AF235); c = _II(c, d, a, b, x[k + 2], S43, 0x2AD7D2BB); b = _II(b, c, d, a, x[k + 9], S44, 0xEB86D391); a = addUnsigned(a, AA); b = addUnsigned(b, BB); c = addUnsigned(c, CC); d = addUnsigned(d, DD); }
    var temp = wordToHex(a) + wordToHex(b) + wordToHex(c) + wordToHex(d); return temp.toLowerCase();
}
function microtime(get_as_float) { var now = new Date().getTime() / 1000; var s = parseInt(now, 10); return (get_as_float) ? now : (Math.round((now - s) * 1000) / 1000) + ' ' + s; }
function min() {
    var ar, retVal, i = 0, n = 0; var argv = arguments, argc = argv.length; var _obj2Array = function (obj) {
        if (obj instanceof Array) { return obj; } else {
            var ar = []; for (var i in obj) { ar.push(obj[i]); }
            return ar;
        }
    }; var _compare = function (current, next) {
        var i = 0, n = 0, tmp = 0; var nl = 0, cl = 0; if (current === next) { return 0; } else if (typeof current == 'object') {
            if (typeof next == 'object') {
                current = _obj2Array(current); next = _obj2Array(next); cl = current.length; nl = next.length; if (nl > cl) { return 1; } else if (nl < cl) { return -1; } else {
                    for (i = 0, n = cl; i < n; ++i) { tmp = _compare(current[i], next[i]); if (tmp == 1) { return 1; } else if (tmp == -1) { return -1; } }
                    return 0;
                }
            } else { return -1; }
        } else if (typeof next == 'object') { return 1; } else if (isNaN(next) && !isNaN(current)) { if (current == 0) { return 0; } else { return (current < 0 ? 1 : -1); } } else if (isNaN(current) && !isNaN(next)) { if (next == 0) { return 0; } else { return (next > 0 ? 1 : -1); } } else { if (next == current) { return 0; } else { return (next > current ? 1 : -1); } }
    }; if (argc === 0) { throw new Error('At least one value should be passed to min()'); } else if (argc === 1) {
        if (typeof argv[0] === 'object') { ar = _obj2Array(argv[0]); } else { throw new Error('Wrong parameter count for min()'); }
        if (ar.length === 0) { throw new Error('Array must contain at least one element for min()'); }
    } else { ar = argv; }
    retVal = ar[0]; for (i = 1, n = ar.length; i < n; ++i) { if (_compare(retVal, ar[i]) == -1) { retVal = ar[i]; } }
    return retVal;
}
function natcasesort(inputArr) {
    var valArr = [], keyArr = [], k, i, ret, that = this, strictForIn = false, populateArr = {}; var bubbleSort = function (keyArr, inputArr) { var i, j, tempValue, tempKeyVal; for (i = inputArr.length - 2; i >= 0; i--) { for (j = 0; j <= i; j++) { ret = that.strnatcasecmp(inputArr[j + 1], inputArr[j]); if (ret < 0) { tempValue = inputArr[j]; inputArr[j] = inputArr[j + 1]; inputArr[j + 1] = tempValue; tempKeyVal = keyArr[j]; keyArr[j] = keyArr[j + 1]; keyArr[j + 1] = tempKeyVal; } } } }; this.php_js = this.php_js || {}; this.php_js.ini = this.php_js.ini || {}; strictForIn = this.php_js.ini['phpjs.strictForIn'] && this.php_js.ini['phpjs.strictForIn'].local_value && this.php_js.ini['phpjs.strictForIn'].local_value !== 'off'; populateArr = strictForIn ? inputArr : populateArr; for (k in inputArr) { if (inputArr.hasOwnProperty(k)) { valArr.push(inputArr[k]); keyArr.push(k); if (strictForIn) { delete inputArr[k]; } } }
    try { bubbleSort(keyArr, valArr); } catch (e) { return false; }
    for (i = 0; i < valArr.length; i++) { populateArr[keyArr[i]] = valArr[i]; }
    return strictForIn || populateArr;
}
function natsort(inputArr) {
    var valArr = [], keyArr = [], k, i, ret, that = this, strictForIn = false, populateArr = {}; var bubbleSort = function (keyArr, inputArr) { var i, j, tempValue, tempKeyVal; for (i = inputArr.length - 2; i >= 0; i--) { for (j = 0; j <= i; j++) { ret = that.strnatcmp(inputArr[j + 1], inputArr[j]); if (ret < 0) { tempValue = inputArr[j]; inputArr[j] = inputArr[j + 1]; inputArr[j + 1] = tempValue; tempKeyVal = keyArr[j]; keyArr[j] = keyArr[j + 1]; keyArr[j + 1] = tempKeyVal; } } } }; this.php_js = this.php_js || {}; this.php_js.ini = this.php_js.ini || {}; strictForIn = this.php_js.ini['phpjs.strictForIn'] && this.php_js.ini['phpjs.strictForIn'].local_value && this.php_js.ini['phpjs.strictForIn'].local_value !== 'off'; populateArr = strictForIn ? inputArr : populateArr; for (k in inputArr) { if (inputArr.hasOwnProperty(k)) { valArr.push(inputArr[k]); keyArr.push(k); if (strictForIn) { delete inputArr[k]; } } }
    try { bubbleSort(keyArr, valArr); } catch (e) { return false; }
    for (i = 0; i < valArr.length; i++) { populateArr[keyArr[i]] = valArr[i]; }
    return strictForIn || populateArr;
}
function nl2br(str, is_xhtml) { var breakTag = (is_xhtml || typeof is_xhtml === 'undefined') ? '<br />' : '<br>'; return (str + '').replace(/([^>\r\n]?)(\r\n|\n\r|\r|\n)/g, '$1' + breakTag + '$2'); }
function number_format(number, decimals, dec_point, thousands_sep) {
    var n = !isFinite(+number) ? 0 : +number, prec = !isFinite(+decimals) ? 0 : Math.abs(decimals), sep = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep, dec = (typeof dec_point === 'undefined') ? '.' : dec_point, s = '', toFixedFix = function (n, prec) { var k = Math.pow(10, prec); return '' + Math.round(n * k) / k; }; s = (prec ? toFixedFix(n, prec) : '' + Math.round(n)).split('.'); if (s[0].length > 3) { s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep); }
    if ((s[1] || '').length < prec) { s[1] = s[1] || ''; s[1] += new Array(prec - s[1].length + 1).join('0'); }
    return s.join(dec);
}
function parse_str(str, array) {
    var glue1 = '=', glue2 = '&', array2 = String(str).split(glue2), i, j, chr, tmp, wtfkey, value, bracket, wtfkeys, evalStr, that = this, fixStr = function (str) { return that.urldecode(str).replace(/([\\"'])/g, '\\$1').replace(/\n/g, '\\n').replace(/\r/g, '\\r'); }; if (!array) { array = this.window; }
    for (i = 0; i < array2.length; i++) {
        tmp = array2[i].split(glue1); if (tmp.length < 2) { tmp = [tmp, '']; }
        wtfkey = fixStr(tmp[0]); value = fixStr(tmp[1]); while (wtfkey.charAt(0) === ' ') { wtfkey = wtfkey.substr(1); }
        if (wtfkey.indexOf('\0') !== -1) { wtfkey = wtfkey.substr(0, wtfkey.indexOf('\0')); }
        if (wtfkey && wtfkey.charAt(0) !== '[') {
            wtfkeys = []; bracket = 0; for (j = 0; j < wtfkey.length; j++) {
                if (wtfkey.charAt(j) === '[' && !bracket) { bracket = j + 1; }
                else if (wtfkey.charAt(j) === ']') {
                    if (bracket) {
                        if (!wtfkeys.length) { wtfkeys.push(wtfkey.substr(0, bracket - 1)); }
                        wtfkeys.push(wtfkey.substr(bracket, j - bracket)); bracket = 0; if (wtfkey.charAt(j + 1) !== '[') { break; }
                    }
                }
            }
            if (!wtfkeys.length) { wtfkeys = [wtfkey]; }
            for (j = 0; j < wtfkeys[0].length; j++) {
                chr = wtfkeys[0].charAt(j); if (chr === ' ' || chr === '.' || chr === '[') { wtfkeys[0] = wtfkeys[0].substr(0, j) + '_' + wtfkeys[0].substr(j + 1); }
                if (chr === '[') { break; }
            }
            evalStr = 'array'; for (j = 0; j < wtfkeys.length; j++) {
                wtfkey = wtfkeys[j]; if ((wtfkey !== '' && wtfkey !== ' ') || j === 0) { wtfkey = "'" + wtfkey + "'"; }
                else { wtfkey = eval(evalStr + '.push([]);') - 1; }
                evalStr += '[' + wtfkey + ']'; if (j !== wtfkeys.length - 1 && eval('typeof ' + evalStr) === 'undefined') { eval(evalStr + ' = [];'); }
            }
            evalStr += " = '" + value + "';\n"; eval(evalStr);
        }
    }
}
function parse_url(str, component) {
    var o = { strictMode: false, key: ["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path", "directory", "file", "query", "anchor"], q: { name: "queryKey", parser: /(?:^|&)([^&=]*)=?([^&]*)/g }, parser: { strict: /^(?:([^:\/?#]+):)?(?:\/\/((?:(([^:@]*):?([^:@]*))?@)?([^:\/?#]*)(?::(\d*))?))?((((?:[^?#\/]*\/)*)([^?#]*))(?:\?([^#]*))?(?:#(.*))?)/, loose: /^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/\/?)?((?:(([^:@]*):?([^:@]*))?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/ } }; var m = o.parser[o.strictMode ? "strict" : "loose"].exec(str), uri = {}, i = 14; while (i--) { uri[o.key[i]] = m[i] || ""; }
    switch (component) {
        case 'PHP_URL_SCHEME': return uri.protocol; case 'PHP_URL_HOST': return uri.host; case 'PHP_URL_PORT': return uri.port; case 'PHP_URL_USER': return uri.user; case 'PHP_URL_PASS': return uri.password; case 'PHP_URL_PATH': return uri.path; case 'PHP_URL_QUERY': return uri.query; case 'PHP_URL_FRAGMENT': return uri.anchor; default: var retArr = {}; if (uri.protocol !== '') { retArr.scheme = uri.protocol; }
            if (uri.host !== '') { retArr.host = uri.host; }
            if (uri.port !== '') { retArr.port = uri.port; }
            if (uri.user !== '') { retArr.user = uri.user; }
            if (uri.password !== '') { retArr.pass = uri.password; }
            if (uri.path !== '') { retArr.path = uri.path; }
            if (uri.query !== '') { retArr.query = uri.query; }
            if (uri.anchor !== '') { retArr.fragment = uri.anchor; }
            return retArr;
    }
}
function pi() { return Math.PI; }
function pow(base, exp) { return Math.pow(base, exp); }
function preg_grep(pattern, input, flags) {
    var p = '', retObj = {}; var invert = (flags === 1 || flags === 'PREG_GREP_INVERT'); if (typeof pattern === 'string') { pattern = eval(pattern); }
    if (invert) { for (p in input) { if (input[p].search(pattern) === -1) { retObj[p] = input[p]; } } } else { for (p in input) { if (input[p].search(pattern) !== -1) { retObj[p] = input[p]; } } }
    return retObj;
}
function print_r(array, return_val) {
    var output = "", pad_char = " ", pad_val = 4, d = this.window.document; var getFuncName = function (fn) {
        var name = (/\W*function\s+([\w\$]+)\s*\(/).exec(fn); if (!name) { return '(Anonymous)'; }
        return name[1];
    }; var repeat_char = function (len, pad_char) {
        var str = ""; for (var i = 0; i < len; i++) { str += pad_char; }
        return str;
    }; var formatArray = function (obj, cur_depth, pad_val, pad_char) {
        if (cur_depth > 0) { cur_depth++; }
        var base_pad = repeat_char(pad_val * cur_depth, pad_char); var thick_pad = repeat_char(pad_val * (cur_depth + 1), pad_char); var str = ""; if (typeof obj === 'object' && obj !== null && obj.constructor && getFuncName(obj.constructor) !== 'PHPJS_Resource') {
            str += "Array\n" + base_pad + "(\n"; for (var key in obj) { if (obj[key] instanceof Array) { str += thick_pad + "[" + key + "] => " + formatArray(obj[key], cur_depth + 1, pad_val, pad_char); } else { str += thick_pad + "[" + key + "] => " + obj[key] + "\n"; } }
            str += base_pad + ")\n";
        } else if (obj === null || obj === undefined) { str = ''; } else { str = obj.toString(); }
        return str;
    }; output = formatArray(array, 0, pad_val, pad_char); if (return_val !== true) {
        if (d.body) { this.echo(output); }
        else {
            try { d = XULDocument; this.echo('<pre xmlns="http://www.w3.org/1999/xhtml" style="white-space:pre;">' + output + '</pre>'); }
            catch (e) { this.echo(output); }
        }
        return true;
    } else { return output; }
}
function rsort(inputArr, sort_flags) {
    var valArr = [], k = '', i = 0, sorter = false, that = this, strictForIn = false, populateArr = []; switch (sort_flags) {
        case 'SORT_STRING': sorter = function (a, b) { return that.strnatcmp(b, a); }; break; case 'SORT_LOCALE_STRING': var loc = this.i18n_loc_get_default(); sorter = this.php_js.i18nLocales[loc].sorting; break; case 'SORT_NUMERIC': sorter = function (a, b) { return (b - a); }; break; case 'SORT_REGULAR': default: sorter = function (a, b) {
            if (a < b) { return 1; }
            if (a > b) { return -1; }
            return 0;
        }; break;
    }
    this.php_js = this.php_js || {}; this.php_js.ini = this.php_js.ini || {}; strictForIn = this.php_js.ini['phpjs.strictForIn'] && this.php_js.ini['phpjs.strictForIn'].local_value && this.php_js.ini['phpjs.strictForIn'].local_value !== 'off'; populateArr = strictForIn ? inputArr : populateArr; for (k in inputArr) { if (inputArr.hasOwnProperty(k)) { valArr.push(inputArr[k]); if (strictForIn) { delete inputArr[k]; } } }
    valArr.sort(sorter); for (i = 0; i < valArr.length; i++) { populateArr[i] = valArr[i]; }
    return strictForIn || populateArr;
}
function shuffle(inputArr) {
    var valArr = [], k = '', i = 0, strictForIn = false, populateArr = []; for (k in inputArr) { if (inputArr.hasOwnProperty(k)) { valArr.push(inputArr[k]); if (strictForIn) { delete inputArr[k]; } } }
    valArr.sort(function () { return 0.5 - my_fix_random(); }); this.php_js = this.php_js || {}; this.php_js.ini = this.php_js.ini || {}; strictForIn = this.php_js.ini['phpjs.strictForIn'] && this.php_js.ini['phpjs.strictForIn'].local_value && this.php_js.ini['phpjs.strictForIn'].local_value !== 'off'; populateArr = strictForIn ? inputArr : populateArr; for (i = 0; i < valArr.length; i++) { populateArr[i] = valArr[i]; }
    return strictForIn || populateArr;
}
function sprintf() {
    var regex = /%%|%(\d+\$)?([-+\'#0 ]*)(\*\d+\$|\*|\d+)?(\.(\*\d+\$|\*|\d+))?([scboxXuidfegEG])/g; var a = arguments, i = 0, format = a[i++]; var pad = function (str, len, chr, leftJustify) {
        if (!chr) { chr = ' '; }
        var padding = (str.length >= len) ? '' : Array(1 + len - str.length >>> 0).join(chr); return leftJustify ? str + padding : padding + str;
    }; var justify = function (value, prefix, leftJustify, minWidth, zeroPad, customPadChar) {
        var diff = minWidth - value.length; if (diff > 0) { if (leftJustify || !zeroPad) { value = pad(value, minWidth, customPadChar, leftJustify); } else { value = value.slice(0, prefix.length) + pad('', diff, '0', true) + value.slice(prefix.length); } }
        return value;
    }; var formatBaseX = function (value, base, prefix, leftJustify, minWidth, precision, zeroPad) { var number = value >>> 0; prefix = prefix && number && { '2': '0b', '8': '0', '16': '0x' }[base] || ''; value = prefix + pad(number.toString(base), precision || 0, '0', false); return justify(value, prefix, leftJustify, minWidth, zeroPad); }; var formatString = function (value, leftJustify, minWidth, precision, zeroPad, customPadChar) {
        if (precision != null) { value = value.slice(0, precision); }
        return justify(value, '', leftJustify, minWidth, zeroPad, customPadChar);
    }; var doFormat = function (substring, valueIndex, flags, minWidth, _, precision, type) {
        var number; var prefix; var method; var textTransform; var value; if (substring == '%%') { return '%'; }
        var leftJustify = false, positivePrefix = '', zeroPad = false, prefixBaseX = false, customPadChar = ' '; var flagsl = flags.length; for (var j = 0; flags && j < flagsl; j++) { switch (flags.charAt(j)) { case ' ': positivePrefix = ' '; break; case '+': positivePrefix = '+'; break; case '-': leftJustify = true; break; case "'": customPadChar = flags.charAt(j + 1); break; case '0': zeroPad = true; break; case '#': prefixBaseX = true; break; } }
        if (!minWidth) { minWidth = 0; } else if (minWidth == '*') { minWidth = +a[i++]; } else if (minWidth.charAt(0) == '*') { minWidth = +a[minWidth.slice(1, -1)]; } else { minWidth = +minWidth; }
        if (minWidth < 0) { minWidth = -minWidth; leftJustify = true; }
        if (!isFinite(minWidth)) { throw new Error('sprintf: (minimum-)width must be finite'); }
        if (!precision) { precision = 'fFeE'.indexOf(type) > -1 ? 6 : (type == 'd') ? 0 : undefined; } else if (precision == '*') { precision = +a[i++]; } else if (precision.charAt(0) == '*') { precision = +a[precision.slice(1, -1)]; } else { precision = +precision; }
        value = valueIndex ? a[valueIndex.slice(0, -1)] : a[i++]; switch (type) { case 's': return formatString(String(value), leftJustify, minWidth, precision, zeroPad, customPadChar); case 'c': return formatString(String.fromCharCode(+value), leftJustify, minWidth, precision, zeroPad); case 'b': return formatBaseX(value, 2, prefixBaseX, leftJustify, minWidth, precision, zeroPad); case 'o': return formatBaseX(value, 8, prefixBaseX, leftJustify, minWidth, precision, zeroPad); case 'x': return formatBaseX(value, 16, prefixBaseX, leftJustify, minWidth, precision, zeroPad); case 'X': return formatBaseX(value, 16, prefixBaseX, leftJustify, minWidth, precision, zeroPad).toUpperCase(); case 'u': return formatBaseX(value, 10, prefixBaseX, leftJustify, minWidth, precision, zeroPad); case 'i': case 'd': number = parseInt(+value, 10); prefix = number < 0 ? '-' : positivePrefix; value = prefix + pad(String(Math.abs(number)), precision, '0', false); return justify(value, prefix, leftJustify, minWidth, zeroPad); case 'e': case 'E': case 'f': case 'F': case 'g': case 'G': number = +value; prefix = number < 0 ? '-' : positivePrefix; method = ['toExponential', 'toFixed', 'toPrecision']['efg'.indexOf(type.toLowerCase())]; textTransform = ['toString', 'toUpperCase']['eEfFgG'.indexOf(type) % 2]; value = prefix + Math.abs(number)[method](precision); return justify(value, prefix, leftJustify, minWidth, zeroPad)[textTransform](); default: return substring; }
    }; return format.replace(regex, doFormat);
}
function str_ireplace(search, replace, subject) {
    var i, k = ''; var searchl = 0; var reg; var escapeRegex = function (s) { return s.replace(/([\\\^\$*+\[\]?{}.=!:(|)])/g, '\\$1'); }; search += ''; searchl = search.length; if (!(replace instanceof Array)) { replace = [replace]; if (search instanceof Array) { while (searchl > replace.length) { replace[replace.length] = replace[0]; } } }
    if (!(search instanceof Array)) { search = [search]; }
    while (search.length > replace.length) { replace[replace.length] = ''; }
    if (subject instanceof Array) {
        for (k in subject) { if (subject.hasOwnProperty(k)) { subject[k] = str_ireplace(search, replace, subject[k]); } }
        return subject;
    }
    searchl = search.length; for (i = 0; i < searchl; i++) { reg = new RegExp(escapeRegex(search[i]), 'gi'); subject = subject.replace(reg, replace[i]); }
    return subject;
}
function str_repeat(input, multiplier) { return new Array(multiplier + 1).join(input); }
function str_replace(search, replace, subject, count) {
    var i = 0, j = 0, temp = '', repl = '', sl = 0, fl = 0, f = [].concat(search), r = [].concat(replace), s = subject, ra = r instanceof Array, sa = s instanceof Array; s = [].concat(s); if (count) { this.window[count] = 0; }
    for (i = 0, sl = s.length; i < sl; i++) {
        if (s[i] === '') { continue; }
        for (j = 0, fl = f.length; j < fl; j++) { temp = s[i] + ''; repl = ra ? (r[j] !== undefined ? r[j] : '') : r[0]; s[i] = (temp).split(f[j]).join(repl); if (count && s[i] !== temp) { this.window[count] += (temp.length - s[i].length) / f[j].length; } }
    }
    return sa ? s : s[0];
}
function strchr(haystack, needle, bool) { return this.strstr(haystack, needle, bool); }
function strip_tags(str, allowed_tags) {
    var wtfkey = '', allowed = false; var matches = []; var allowed_array = []; var allowed_tag = ''; var i = 0; var k = ''; var html = ''; var replacer = function (search, replace, str) { return str.split(search).join(replace); }; if (allowed_tags) { allowed_array = allowed_tags.match(/([a-zA-Z0-9]+)/gi); }
    str += ''; matches = str.match(/(<\/?[\S][^>]*>)/gi); for (wtfkey in matches) {
        if (isNaN(wtfkey)) { continue; }
        html = matches[wtfkey].toString(); allowed = false; for (k in allowed_array) {
            allowed_tag = allowed_array[k]; i = -1; if (i != 0) { i = html.toLowerCase().indexOf('<' + allowed_tag + '>'); }
            if (i != 0) { i = html.toLowerCase().indexOf('<' + allowed_tag + ' '); }
            if (i != 0) { i = html.toLowerCase().indexOf('</' + allowed_tag); }
            if (i == 0) { allowed = true; break; }
        }
        if (!allowed) { str = replacer(html, "", str); }
    }
    return str;
}
function stripos(f_haystack, f_needle, f_offset) {
    var haystack = (f_haystack + '').toLowerCase(); var needle = (f_needle + '').toLowerCase(); var index = 0; if ((index = haystack.indexOf(needle, f_offset)) !== -1) { return index; }
    return false;
}
function strchr(haystack, needle, bool) { return this.strstr(haystack, needle, bool); }
function stripslashes(str) { return (str + '').replace(/\\(.?)/g, function (s, n1) { switch (n1) { case '\\': return '\\'; case '0': return '\u0000'; case '': return ''; default: return n1; } }); }
function strlen(string) {
    var str = string + ''; var i = 0, chr = '', lgth = 0; if (!this.php_js || !this.php_js.ini || !this.php_js.ini['unicode.semantics'] || this.php_js.ini['unicode.semantics'].local_value.toLowerCase() !== 'on') { return string.length; }
    var getWholeChar = function (str, i) {
        var code = str.charCodeAt(i); var next = '', prev = ''; if (0xD800 <= code && code <= 0xDBFF) {
            if (str.length <= (i + 1)) { throw 'High surrogate without following low surrogate'; }
            next = str.charCodeAt(i + 1); if (0xDC00 > next || next > 0xDFFF) { throw 'High surrogate without following low surrogate'; }
            return str.charAt(i) + str.charAt(i + 1);
        } else if (0xDC00 <= code && code <= 0xDFFF) {
            if (i === 0) { throw 'Low surrogate without preceding high surrogate'; }
            prev = str.charCodeAt(i - 1); if (0xD800 > prev || prev > 0xDBFF) { throw 'Low surrogate without preceding high surrogate'; }
            return false;
        }
        return str.charAt(i);
    }; for (i = 0, lgth = 0; i < str.length; i++) {
        if ((chr = getWholeChar(str, i)) === false) { continue; }
        lgth++;
    }
    return lgth;
}
function strpos(haystack, needle, offset) { var i = (haystack + '').indexOf(needle, (offset || 0)); return i === -1 ? false : i; }
function strrchr(haystack, needle) {
    var pos = 0; if (typeof needle !== 'string') { needle = String.fromCharCode(parseInt(needle, 10)); }
    needle = needle.charAt(0); pos = haystack.lastIndexOf(needle); if (pos === -1) { return false; }
    return haystack.substr(pos);
}
function strstr(haystack, needle, bool) { var pos = 0; haystack += ''; pos = haystack.indexOf(needle); if (pos == -1) { return false; } else { if (bool) { return haystack.substr(0, pos); } else { return haystack.slice(pos); } } }
function strtolower(str) { return (str + '').toLowerCase(); }
function strtotime(str, now) {
    var i, match, s, strTmp = '', parse = ''; strTmp = str; strTmp = strTmp.replace(/\s{2,}|^\s|\s$/g, ' '); strTmp = strTmp.replace(/[\t\r\n]/g, ''); if (strTmp == 'now') { return (new Date()).getTime() / 1000; } else if (!isNaN(parse = Date.parse(strTmp))) { return (parse / 1000); } else if (now) { now = new Date(now * 1000); } else { now = new Date(); }
    strTmp = strTmp.toLowerCase(); var __is = { day: { 'sun': 0, 'mon': 1, 'tue': 2, 'wed': 3, 'thu': 4, 'fri': 5, 'sat': 6 }, mon: { 'jan': 0, 'feb': 1, 'mar': 2, 'apr': 3, 'may': 4, 'jun': 5, 'jul': 6, 'aug': 7, 'sep': 8, 'oct': 9, 'nov': 10, 'dec': 11 } }; var process = function (m) {
        var ago = (m[2] && m[2] == 'ago'); var num = (num = m[0] == 'last' ? -1 : 1) * (ago ? -1 : 1); switch (m[0]) {
            case 'last': case 'next': switch (m[1].substring(0, 3)) {
                case 'yea': now.setFullYear(now.getFullYear() + num); break; case 'mon': now.setMonth(now.getMonth() + num); break; case 'wee': now.setDate(now.getDate() + (num * 7)); break; case 'day': now.setDate(now.getDate() + num); break; case 'hou': now.setHours(now.getHours() + num); break; case 'min': now.setMinutes(now.getMinutes() + num); break; case 'sec': now.setSeconds(now.getSeconds() + num); break; default: var day; if (typeof (day = __is.day[m[1].substring(0, 3)]) != 'undefined') {
                    var diff = day - now.getDay(); if (diff == 0) { diff = 7 * num; } else if (diff > 0) { if (m[0] == 'last') { diff -= 7; } } else { if (m[0] == 'next') { diff += 7; } }
                    now.setDate(now.getDate() + diff);
                }
            }
                break; default: if (/\d+/.test(m[0])) { num *= parseInt(m[0], 10); switch (m[1].substring(0, 3)) { case 'yea': now.setFullYear(now.getFullYear() + num); break; case 'mon': now.setMonth(now.getMonth() + num); break; case 'wee': now.setDate(now.getDate() + (num * 7)); break; case 'day': now.setDate(now.getDate() + num); break; case 'hou': now.setHours(now.getHours() + num); break; case 'min': now.setMinutes(now.getMinutes() + num); break; case 'sec': now.setSeconds(now.getSeconds() + num); break; } } else { return false; }
                break;
        }
        return true;
    }; match = strTmp.match(/^(\d{2,4}-\d{2}-\d{2})(?:\s(\d{1,2}:\d{2}(:\d{2})?)?(?:\.(\d+))?)?$/); if (match != null) {
        if (!match[2]) { match[2] = '00:00:00'; } else if (!match[3]) { match[2] += ':00'; }
        s = match[1].split(/-/g); for (i in __is.mon) { if (__is.mon[i] == s[1] - 1) { s[1] = i; } }
        s[0] = parseInt(s[0], 10); s[0] = (s[0] >= 0 && s[0] <= 69) ? '20' + (s[0] < 10 ? '0' + s[0] : s[0] + '') : (s[0] >= 70 && s[0] <= 99) ? '19' + s[0] : s[0] + ''; return parseInt(this.strtotime(s[2] + ' ' + s[1] + ' ' + s[0] + ' ' + match[2]) + (match[4] ? match[4] / 1000 : ''), 10);
    }
    var regex = '([+-]?\\d+\\s' + '(years?|months?|weeks?|days?|hours?|min|minutes?|sec|seconds?' + '|sun\\.?|sunday|mon\\.?|monday|tue\\.?|tuesday|wed\\.?|wednesday' + '|thu\\.?|thursday|fri\\.?|friday|sat\\.?|saturday)' + '|(last|next)\\s' + '(years?|months?|weeks?|days?|hours?|min|minutes?|sec|seconds?' + '|sun\\.?|sunday|mon\\.?|monday|tue\\.?|tuesday|wed\\.?|wednesday' + '|thu\\.?|thursday|fri\\.?|friday|sat\\.?|saturday))' + '(\\sago)?'; match = strTmp.match(new RegExp(regex, 'gi')); if (match == null) { return false; }
    for (i = 0; i < match.length; i++) { if (!process(match[i].split(' '))) { return false; } }
    return (now.getTime() / 1000);
}
function strtoupper(str) { return (str + '').toUpperCase(); }
function substr(str, start, len) {
    var i = 0, allBMP = true, es = 0, el = 0, se = 0, ret = ''; str += ''; var end = str.length; this.php_js = this.php_js || {}; this.php_js.ini = this.php_js.ini || {}; switch ((this.php_js.ini['unicode.semantics'] && this.php_js.ini['unicode.semantics'].local_value.toLowerCase())) {
        case 'on': for (i = 0; i < str.length; i++) { if (/[\uD800-\uDBFF]/.test(str.charAt(i)) && /[\uDC00-\uDFFF]/.test(str.charAt(i + 1))) { allBMP = false; break; } }
            if (!allBMP) {
                if (start < 0) { for (i = end - 1, es = (start += end); i >= es; i--) { if (/[\uDC00-\uDFFF]/.test(str.charAt(i)) && /[\uD800-\uDBFF]/.test(str.charAt(i - 1))) { start--; es--; } } }
                else {
                    var surrogatePairs = /[\uD800-\uDBFF][\uDC00-\uDFFF]/g; while ((surrogatePairs.exec(str)) != null) {
                        var li = surrogatePairs.lastIndex; if (li - 2 < start) { start++; }
                        else { break; }
                    }
                }
                if (start >= end || start < 0) { return false; }
                if (len < 0) {
                    for (i = end - 1, el = (end += len); i >= el; i--) { if (/[\uDC00-\uDFFF]/.test(str.charAt(i)) && /[\uD800-\uDBFF]/.test(str.charAt(i - 1))) { end--; el--; } }
                    if (start > end) { return false; }
                    return str.slice(start, end);
                }
                else {
                    se = start + len; for (i = start; i < se; i++) { ret += str.charAt(i); if (/[\uD800-\uDBFF]/.test(str.charAt(i)) && /[\uDC00-\uDFFF]/.test(str.charAt(i + 1))) { se++; } }
                    return ret;
                }
                break;
            }
        case 'off': default: if (start < 0) { start += end; }
            end = typeof len === 'undefined' ? end : (len < 0 ? len + end : len + start); return start >= str.length || start < 0 || start > end ? !1 : str.slice(start, end);
    }
    return undefined;
}
function trim(str, charlist) {
    var whitespace, l = 0, i = 0; str += ''; if (!charlist) { whitespace = " \n\r\t\f\x0b\xa0\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u200b\u2028\u2029\u3000"; } else { charlist += ''; whitespace = charlist.replace(/([\[\]\(\)\.\?\/\*\{\}\+\$\^\:])/g, '$1'); }
    l = str.length; for (i = 0; i < l; i++) { if (whitespace.indexOf(str.charAt(i)) === -1) { str = str.substring(i); break; } }
    l = str.length; for (i = l - 1; i >= 0; i--) { if (whitespace.indexOf(str.charAt(i)) === -1) { str = str.substring(0, i + 1); break; } }
    return whitespace.indexOf(str.charAt(0)) === -1 ? str : '';
}
function urldecode(str) { return decodeURIComponent(str.replace(/\+/g, '%20')); }
function urlencode(str) { str = (str + '').toString(); return encodeURIComponent(str).replace(/!/g, '%21').replace(/'/g, '%27').replace(/\(/g, '%28').replace(/\)/g, '%29').replace(/\*/g, '%2A').replace(/%20/g, '+'); }
function is_array(mixed_var) {
    var ini,
        _getFuncName = function (fn) {
            var name = (/\W*function\s+([\w\$]+)\s*\(/).exec(fn);
            if (!name) {
                return '(Anonymous)';
            }
            return name[1];
        },
        _isArray = function (mixed_var) {
            if (!mixed_var || typeof mixed_var !== 'object' || typeof mixed_var.length !== 'number') {
                return false;
            }
            var len = mixed_var.length;
            mixed_var[mixed_var.length] = 'bogus';
            if (len !== mixed_var.length) {
                mixed_var.length -= 1;
                return true;
            }
            delete mixed_var[mixed_var.length];
            return false;
        };

    if (!mixed_var || typeof mixed_var !== 'object') {
        return false;
    }
    this.php_js = this.php_js || {};
    this.php_js.ini = this.php_js.ini || {};
    ini = this.php_js.ini['phpjs.objectsAsArrays'];

    return _isArray(mixed_var) ||
        ((!ini || (
            (parseInt(ini.local_value, 10) !== 0 && (!ini.local_value.toLowerCase || ini.local_value.toLowerCase() !== 'off')))
        ) && (
                Object.prototype.toString.call(mixed_var) === '[object Object]' && _getFuncName(mixed_var.constructor) === 'Object' // Most likely a literal and intended as assoc. array
            ));
}
function time() { return Math.floor(new Date().getTime() / 1000); }
function utf8_decode(str_data) {
    var tmp_arr = [], i = 0, ac = 0, c1 = 0, c2 = 0, c3 = 0; str_data += ''; while (i < str_data.length) { c1 = str_data.charCodeAt(i); if (c1 < 128) { tmp_arr[ac++] = String.fromCharCode(c1); i++; } else if ((c1 > 191) && (c1 < 224)) { c2 = str_data.charCodeAt(i + 1); tmp_arr[ac++] = String.fromCharCode(((c1 & 31) << 6) | (c2 & 63)); i += 2; } else { c2 = str_data.charCodeAt(i + 1); c3 = str_data.charCodeAt(i + 2); tmp_arr[ac++] = String.fromCharCode(((c1 & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63)); i += 3; } }
    return tmp_arr.join('');
}
function utf8_encode(argString) {
    var string = (argString + ''); var utftext = ""; var start, end; var stringl = 0; start = end = 0; stringl = string.length; for (var n = 0; n < stringl; n++) {
        var c1 = string.charCodeAt(n); var enc = null; if (c1 < 128) { end++; } else if (c1 > 127 && c1 < 2048) { enc = String.fromCharCode((c1 >> 6) | 192) + String.fromCharCode((c1 & 63) | 128); } else { enc = String.fromCharCode((c1 >> 12) | 224) + String.fromCharCode(((c1 >> 6) & 63) | 128) + String.fromCharCode((c1 & 63) | 128); }
        if (enc !== null) {
            if (end > start) { utftext += string.substring(start, end); }
            utftext += enc; start = end = n + 1;
        }
    }
    if (end > start) { utftext += string.substring(start, string.length); }
    return utftext;
}
function array_slice(arr, offst, lgth, preserve_keys) {
    var wtfkey = ''; if (!(arr instanceof Array) || (preserve_keys && offst !== 0)) {
        var lgt = 0, newAssoc = {}; for (wtfkey in arr) { lgt += 1; newAssoc[wtfkey] = arr[wtfkey]; }
        arr = newAssoc; offst = (offst < 0) ? lgt + offst : offst; lgth = lgth === undefined ? lgt : (lgth < 0) ? lgt + lgth - offst : lgth; var assoc = {}; var start = false, it = -1, arrlgth = 0, no_pk_idx = 0; for (wtfkey in arr) {
            ++it; if (arrlgth >= lgth) { break; }
            if (it == offst) { start = true; }
            if (!start) { continue; }
            ++arrlgth; if (this.is_int(wtfkey) && !preserve_keys) { assoc[no_pk_idx++] = arr[wtfkey]; } else { assoc[wtfkey] = arr[wtfkey]; }
        }
        return assoc;
    }
    if (lgth === undefined) { return arr.slice(offst); } else if (lgth >= 0) { return arr.slice(offst, offst + lgth); } else { return arr.slice(offst, lgth); }
}
function ceil(value) { return Math.ceil(value); }
function array_reverse(array, preserve_keys) { var arr_len = array.length, newkey = 0, tmp_arr = {}, key = ''; preserve_keys = !!preserve_keys; for (key in array) { newkey = arr_len - key - 1; tmp_arr[preserve_keys ? key : newkey] = array[key]; } return tmp_arr; }