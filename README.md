# prj-csharp-loadtest-api
Chaos monkey style API response to test a load testing program against

Creates random delays when returning responses 1-4 seconds designed so that 90% fall between 1-2 \
~80% pass rate with JSON response {successful:true} \
~10% response failure rate \
~10% rate returning {successful:false}
