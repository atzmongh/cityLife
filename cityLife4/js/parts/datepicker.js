$( function() {
    var dateFormat = "d MM, yy",
        from = $( "#fromDate" )
            .datepicker({
                defaultDate: "+1w",
                numberOfMonths: 1,
                dateFormat: "d MM, yy",
                firstDay: 1
            })
            .on( "change", function() {
                to.datepicker( "option", "minDate", getDate( this ) );
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