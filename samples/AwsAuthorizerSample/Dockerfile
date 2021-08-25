FROM public.ecr.aws/lambda/dotnet:5.0

WORKDIR /var/task

COPY "sample_output" .

# There is no need to define the CMD in the dockerfile, do it at Lambda image config
#CMD [ "" ]