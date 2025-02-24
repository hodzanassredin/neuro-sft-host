cmake --build . --target run_local
echo llm server http://localhost:8080/
echo backend https://localhost:5001/
echo frontend https://localhost:5002/
echo WARNING you probably need to go to front and back and accept self signed certs