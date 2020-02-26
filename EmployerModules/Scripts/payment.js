
        const total_payment = $("#t-payment").val();
        const employerId = $("#empId").val();
        const employerName = $("#empName").val();
        const employerEmail = $("#empEmail").val();
        const employerPhoneNo = $("#empPhoneNo").val();
        const period = $("#period").val();
        const pk = $('#pk').val()

       // Paystack Payment gateway
      function payWithPaystack(){
        var handler = PaystackPop.setup({
        key: $("#ps_pk").val(),
            email: employerEmail,
            amount: Number(total_payment) * 100,
            //amount: parseInt(40000) * 100,
            //plan: "PLN_code",
            //ref: "UNIQUE TRANSACTION REFERENCE HERE",
          metadata: {
        custom_fields: [
                {
        display_name: "Employer ID",
                     variable_name: "employer_id",
                     value: employerId
                 },
                 {
        display_name: "Desecription",
                     variable_name: "desc",
                     value: "Payment for " + period + " by " + employerId
                 },
             ]
          },
            callback: function (response) {
        //alert('successfully subscribed. transaction ref is ' + response.reference);
        let verify = verifyPayment(response.reference);
                //console.log(verify.data.status)
                if (verify.data.status == "success" ) {
        let upt = updateTables()
                    window.open("http://localhost:61383/Employer/RecentTransaction", "_self")
                }
          },
          onClose: function(){
        alert('window closed');
    }
                    });
        handler.openIframe();
        }

        function verifyPayment(ref) {
            const paystackheader = "Bearer " + $("#ps_sk").val();
            let resp = {}

    $.ajax
                ({
        type: "GET",
                    url: "https://api.paystack.co/transaction/verify/" + ref,
                    dataType: 'json',
                    async: false,
                    data: '{}',
                    beforeSend: function (xhr) {
        xhr.setRequestHeader('Authorization', paystackheader);
    },
                    success: function (res) {
        resp = res

    }
    });
            return resp
        }

        function updateTables() {
        $.ajax({
            //url: '@Url.Action("PaymentConfirmed", "Employer", new { paymentType="paystack"})',
            url: '/Employer/PaymentConfirmed?paymentType="paystack"',
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (results) {
                //alert(results)
            },
            error: function (err) {
                alert('Error occured while updating tables');
                console.log(err)
            }
        });

    }




        // Asp.net Payment integration

            $(function () {
        function InitTransaction(data) {
            return $.ajax({
                type: "POST",
                //url: "@Url.Action("InitializePayment", "Employer", new { area=""})",
                url: '/Employer/InitializePayment?area=""',
                data: data,
                dataType: 'json',
                contentType: 'application/json;charset=utf-8'
            });
        }

                $("#serverPaystack").click(function (e) {
                
                    console.log("I was clicked")
                $(".redirect-message").show();
                e.preventDefault();

                var data = JSON.stringify({
                EmployerName: $("#empName").val(),
                employerId: $("#empId").val(),
                email: $("#empEmail").val(),
                phone: employerPhoneNo,
                //amount: 18703833
                amount: Number(total_payment) * 100,
                 });
                    $.when(InitTransaction(data)).then(function (response) {
                        console.log(response)
                        if (response.error == false) {
                window.location.href = response.result.data.authorization_url;
                console.log(response.result.data.authorization_url);
                        } else {
                $(".redirect-message").hide();
            }
                    }).fail(function () {
                $(".redirect-message").hide()
            });
                });
        });









        // Flutterwave payment Gateway
        const API_publicKey = $("#fw_pk").val();

        function payWithRave() {
            var x = getpaidSetup({
            PBFPubKey: API_publicKey,
                customer_email: employerEmail,
                amount: Number(total_payment),
                //amount: 40000,
                customer_phone: employerPhoneNo,
                txref: $("#txref").val(),
                currency: "NGN",
                meta: [
                    {
                    metaname: "EmployerID",
                                metavalue: employerId
                                },
                                {
                    metaname: "EmployerName",
                                    metavalue: employerName
                                },
                                {
                    metaname: "Description",
                                    metavalue: "Payment for " + period + " by " + employerName + "with  Employer Id" + employerId
                                }
                            ],
                onclose: function () {},
                callback: function (response) {
                    var txref = response.tx.txRef; // collect txRef returned and pass to a server page to complete status check.
                    //console.log("This is the response returned after a charge", response);
                    if (
                        response.tx.chargeResponseCode == "00" ||
                        response.tx.chargeResponseCode == "0"
                    ) {
        // redirect to a success page
        console.log("txref is", txref);
    window.location.replace("/Employer/Validate?TransactionId=" + txref);
                        //var check = verifyFlutterWavePayment(txref);

                        //alert("Successful")

                    } else {
        // redirect to a failure page.
        alert("Payment Failed");
    }

                    x.close(); // use this to close the modal immediately after payment.
                }
            });
        }

        function verifyFlutterWavePayment(ref) {

        $.ajax
            ({
                type: "POST",
                url: "https://ravesandboxapi.flutterwave.com/flwv3-pug/getpaidx/api/xrequery",

                data: {
                    SECKEY: $("#fw_sk").val(),
                    txref: ref
                },
                //beforeSend: function (xhr) {
                //    xhr.setRequestHeader('Authorization', paystackheader);
                //},
                success: function (res) {
                    console.log("Your Tumininu reponse", res)
                    if (res.status == "success" && res.data.amount == 40958) {
                        console.log("Redirect to updatetable");

                        $("#loading").show();

                        updateFlutterTables();
                    }
                    return res
                }
            });

    }

function updateFlutterTables() {
    $.ajax({
        //url: '@Url.Action("PaymentConfirmed", "Employer", new { paymentType="flutterwave"})',
        url: '/Employer/PaymentConfirmed?paymentType="paystack"',
        type: 'GET',
        dataType: 'json',
        cache: false,
        success: function (results) {
            window.open("http://localhost:61383/Employer/RecentTransaction", "_self")
        },
        error: function (err) {
            alert('Error occured while updating tables');
            console.log(err)
        }
    });

}