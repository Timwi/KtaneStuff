(function(list)
{
    function addLog(x)
    {
        console.log(x);
    }

    var socket = new WebSocket("ws://localhost:8992/websocket");
    socket.onopen = function()
    {
        addLog('Socket opened.');
    };
    socket.onclose = function()
    {
        addLog('Socket closed.');
    };
    socket.onerror = function(err)
    {
        addLog('Socket error: ' + err);
        console.log(err);
    };
    socket.onmessage = function(msg)
    {
        // msg.data
    };

    for (var i = 0; i < list.length; i++)
        list[i].onclick = function(e)
        {
            var selectorArr = [];
            var elem = this;
            while (elem !== null && elem !== document.body.parentElement)
            {
                var parent = elem.parentElement;
                selectorArr.unshift(parent === null || parent === document.body.parentElement
                    ? elem.nodeName.toLowerCase()
                    : `${elem.nodeName.toLowerCase()}:nth-child(${Array.indexOf(parent.children, elem) + 1})`);
                elem = parent;
            }

            var str1 = 'translatable';
            var str2 = 'translatable-mod';

            var curClasses = this.className.split(' ');
            var pos1 = curClasses.indexOf(str1);
            var pos2 = curClasses.indexOf(str2);
            if (pos1 !== -1)
            {
                curClasses.splice(pos1, 1);
                curClasses.push(str2);
                socket.send(JSON.stringify([selectorArr.join('>'), str1, str2]));
            }
            else if (pos2 !== -1)
            {
                curClasses.splice(pos2, 1);
                socket.send(JSON.stringify([selectorArr.join('>'), str2, null]));
            }
            else
            {
                curClasses.push(str1);
                socket.send(JSON.stringify([selectorArr.join('>'), null, str1]));
            }
            this.className = curClasses.join(' ');
            e.stopPropagation();
        };
})(document.querySelectorAll('td,th,li,p,span,div,h1,h2,h3,h4,h5,h6,caption'));
