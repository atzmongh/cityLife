$( function() {
    var dateFormat = "d MM, yy",
        from = $( "#fromDate" )
            .datepicker({
                defaultDate: "+0d",
                minDate: timeNow,   //The timenow is defined in a script before this script, and it gives the fake time now
                numberOfMonths: 1,
                dateFormat: "d MM, yy",
                firstDay: 1
            })
            .on("change", function () {
                var fromDate = getDate(this);   //get the "from date"
                var nextDay = fromDate.getDate() + 1;  //get the day component and add 1 to it.
                var nextDate = new Date(fromDate);
                nextDate.setDate(nextDay);             //Add the day component to the "from date". Note that if from date is 31/12/2018, it will skip to 1/1/2019
                to.datepicker("option", "minDate", nextDate);
            }),
        to = $( "#toDate" ).datepicker({
            defaultDate: "+1w",
            numberOfMonths: 1,
            dateFormat: "d MM, yy",
            firstDay: 1
        })
            .on( "change", function() {
                from.datepicker( "option", "maxDate", getDate( this ) );
            });

    function getDate( element ) {
        var date;
        try {
            date = $.datepicker.parseDate( dateFormat, element.value );
        } catch( error ) {
            date = null;
        }

        return date;
    }
} );


//$('.ui-datepicker-calendar').insertAfter('.datepicker-note');