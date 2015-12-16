// Timer progress bar

var timer = 0,
    perc = 0,
    timeTotal = 3000,
    timeCount = 1,
    cFlag;

function updateProgress(percentage) {
    var x = (percentage/timeTotal)*100,
        y = x.toFixed(1);
    $('#pbar_innerdiv').css("width", x + "%");
    $('#pbar_innertext').text(y + "%");
}

function animateUpdate() {
    if(perc < timeTotal) {
        perc++;
        updateProgress(perc);
        timer = setTimeout(animateUpdate, timeCount);
    } else if (perc == timeTotal) {
        $('#pbar_donetext').css("display", "block");
    }
}



$(document).ready(function() {
    $("#pbar_outerdiv").click(function() {
        if (cFlag == undefined) {
            clearTimeout(timer);
            perc = 0;
            cFlag = true;
            animateUpdate();
        }
        else if (!cFlag) {
            cFlag = true;
            animateUpdate();
        }
        else {
            clearTimeout(timer);
            cFlag = false;
        }
    });

    $(".item").click(function() {
        if($(".extra").css('display') == 'none') {
            $(".extra").fadeIn(300).show(); 
        }
        else if ($(".extra").css('display') == 'block') {
            $(".extra").fadeOut(300).hide();
      }
    });

    // Assign roles

    $("#assign-1").click(function() {
        $("#assign-1").text(function(i, text){

               return text === 'SAAD' ? 'JACK' : 'SAAD'
            })
    });

    $("#assign-2").click(function() {
        $("#assign-2").text(function(i, text){
               return text === 'SAAD' ? 'JACK' : 'SAAD'
            })
    });

    $("#assign-3").click(function() {
        $("#assign-3").text(function(i, text){
               return text === 'SAAD' ? 'JACK' : 'SAAD'
            })
    });

    $("#assign-4").click(function() {
        $("#assign-4").text(function(i, text){
               return text === 'SAAD' ? 'JACK' : 'SAAD'
            })
    });

    // Switch prev/next step when people are separated
    $("#step-1").click(function() {
        $("#step-1").css("display", "none");
        $("#step-2").css("display", "table");
    });
     $("#step-2").click(function() {
        $("#step-2").css("display", "none");
        $("#step-1").css("display", "table");
    });
     
}); 