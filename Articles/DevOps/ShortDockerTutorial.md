# Docker tutorial - building and running
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Docker, CI/CD">
</Categories>

Here is a small tutorial for getting started with Docker. The key difference between this tutorial and many others is that all unnecessary complexity has been shaved away.

Table of contents

   * [Images and containers](#images-and-containers)
   * [Creating our first image](#creating-our-first-image)
   * [Build the image](#build-the-image)
   * [See the result](#see-the-result)
   * [Save the image to a file on the filesystem](#save-the-image-to-a-file-on-the-filesystem)
   * [Run the image as a container](#run-the-image-as-a-container)
   * ['CMD' - automatically run a command](#cmd---automatically-run-a-command)
   * ['ENTRYPOINT' - automatically run a command that cannot be changed](#entrypoint---automatically-run-a-command-that-cannot-be-changed)
   * ['COPY' / 'ADD' - add artifacts to the image](#copy--add---add-artifacts-to-the-image)
   * [Next steps for the tutorial](#next-steps-for-the-tutorial)


## Images and containers
In programming, we have classes and objects that are instances of classes. In the Docker world, an image is comparable to a class, and a container is comparable to an object.

## Creating our first image

Create a file named `Dockerfile` in an empty folder with this content:

```
FROM ubuntu:latest
```


## Build the image

In the same folder, run:

```
docker build -t my-stuff:1.0 -t my-stuff:latest .
```


Flags explained:

* `-t my-stuff:1.0` = a tag, meaning the name and version of your image
* `-t my-stuff:latest` = another tag for the same image
* `.` = build from the Dockerfile in the current folder

We tag the image so we can find it again easily. We use two tags, so we have both a versioned tag and a `latest` tag. The `latest` tag makes it easy to refer to the most recent build, similar to how we use `FROM ubuntu:latest`.


## See the result
Looking in the current folder will not show anything new:

```
> dir

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    08:16             37 dockerfile
```

The image is only visible inside Docker's internal image store:

```
> docker image ls

REPOSITORY                 TAG       IMAGE ID       CREATED        SIZE
my-stuff                   1.0       4e759ff0d3fd   13 days ago    157MB
my-stuff                   latest    4e759ff0d3fd   13 days ago    157MB
...
```


## Save the image to a file on the filesystem
We can save the image to the filesystem. Docker saves images as tar files. Other formats, such as zip files, are not supported by `docker save`.


```
> docker save -o my.tar my-stuff:1.0
> dir

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    08:16             37 dockerfile
-a---          05-05-2026    08:25       41.574.400 my.tar
```


## Run the image as a container

To create a container from an image and get an interactive shell in Ubuntu Linux, run:

```
> docker run -it my-stuff:1.0

root@2bd442a1e2d1:/#
```

`-it` is important because Bash is interactive. Without it, Bash will stop immediately.

* `-i` keeps input open
* `-t` creates a terminal

We can also run the container with the command `ls` to see the content of the startup folder:

```
> docker run -t my-stuff:1.0 ls

bin  boot  dev  etc  home  lib  lib64  media  mnt  opt  proc  root  run  sbin  srv  sys  tmp  usr  var
```


## 'CMD' - automatically run a command

`CMD` defines the command that runs when the container starts. Only one `CMD` instruction can be used. If the Dockerfile contains multiple `CMD` instructions, only the last one is used.

Let's modify our `Dockerfile` to:

```
FROM ubuntu:latest
CMD echo "First" && echo "Second" && echo "Third"
```

Build and run the new image:

```
> docker build -t my-stuff:3.0 -t my-stuff:latest .
> docker run my-stuff:latest

First
Second
Third
```

`CMD` can be overridden. We can replace the command at runtime:

```
> docker run -t my-stuff:latest ls

bin  boot  dev  etc  home  lib  lib64  media  mnt  opt  proc  root  run  sbin  srv  sys  tmp  usr  var
```

## 'ENTRYPOINT' - automatically run a command that cannot be changed

The entrypoint is a command that runs on startup. When both `ENTRYPOINT` and `CMD` are used, `CMD` acts as the default parameter to `ENTRYPOINT`.

Change the `Dockerfile` to:

```
FROM ubuntu:latest
ENTRYPOINT ["echo"]
CMD ["Hello World"]
```

```
> docker build -t my-stuff:latest .
> docker run my-stuff:latest

Hello World
```

If we use a command parameter such as `ls`, as we did earlier, we no longer get the folder content:

```
> docker run my-stuff:latest ls

ls
```

This happens because `ls` is now passed as a parameter to `echo`.



## 'COPY' / 'ADD' - add artifacts to the image
Now that the basics are in place, we can focus on putting a program into the Docker image. Use `ADD` or `COPY` for this. `ADD` can fetch resources from the internet and unpack local tar files, which makes it less predictable than `COPY`. Prefer `COPY` when possible.

Let's make a small program that prints the numbers 1 to 10. For now, we can imagine that this is a more complex application.

Create a new file named `print_numbers.sh` with this content:

```
#!/bin/sh
for i in $(seq 1 10); do echo "$i"; done
```

If you are on Windows, make sure the file uses Unix-style line endings.

Change the `Dockerfile` to:

```
FROM ubuntu:latest
COPY print_numbers.sh /app/
RUN chmod +x /app/print_numbers.sh
CMD ["/app/print_numbers.sh"]
```

We copy our application to the `/app/` folder to separate it from the tools we may need to install later. We use `chmod` to make the script executable. Finally, we set it as the default command using `CMD`.

```
> docker build -t my-stuff:latest .
> docker run my-stuff:latest

1
2
...
```

