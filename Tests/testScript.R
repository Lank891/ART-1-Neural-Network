install.packages("dplyr")
library(dplyr)
#install.packages("knitr")
#library(knitr)
remove.packages("ggplot2") # Unisntall ggplot
install.packages("ggplot2") # Install it again
library(ggplot2) # Load the librarie (you have to do this one on each new session)
install.packages("fields")
library("fields")
install.packages("ggmosaic")
library("ggmosaic")
#install.packages("lemon")
#library("lemon")

#load the data
train <- data.frame(read.csv(file = '../bin/Debug/net6.0/mnist_train.csv',header=FALSE))
test <- data.frame(read.csv(file = '../bin/Debug/net6.0/mnist_test.csv',header=FALSE))

#read the output
output <- data.frame(read.csv(file = '../bin/Debug/net6.0/output.csv',header=FALSE))
outOcc <-data.frame(table(output$V1))
clNo = 22

#calculate occurences
occ <- data.frame(table(train$V1),table(test$V1))
occ <- subset(occ, select = -c(Var1.1) )
colnames(occ) <- c("Digit","TrainCount","TestCount")
TestToTrainQuot=round(occ$TestCount/(occ$TrainCount+occ$TestCount),3)
occ <- data.frame(occ,TestToTrainQuot)

#plot the vigilance parameter
vig <- data.frame(VigPar=c(0.00001,0.0001,0.001,0.002,0.003,0.004,0.0045,0.005,0.006,0.008,0.01,0.015,0.02),ClustersCount=c(22,22,22,22,22,22,24,26,26,31,41,62,87))
plot(ClustersCount~VigPar,data=vig,main="Vigilance Parameter vs Number of Clusters",xlab="Vigilance Parameter",ylab="Number of Clusters")
lines(x=vig$VigPar,y=vig$ClustersCount,col=rgb(1,0,0,3/4))#,0.015,58, 0.02, 97

#count the frequency
outVersTest <- data.frame(outCluster=output$V1,testDigit=test$V1)
h_s = outVersTest %>% 
  group_by(outCluster,testDigit) %>% 
  summarise(count=n())
temp<-data.frame(outCluster=rep(seq(from=0,to=(clNo-1),by=1),each=10,times=1),testDigit=rep(seq(from=0,to=9,by=1),times=clNo),count=numeric(clNo*10))

#add frequency to the data frame with unique pairs
for (i in 1:length(temp$outCluster)) {
  for (j in 1:length(h_s$outCluster)) {
    if(temp$outCluster[i]==h_s$outCluster[j] && temp$testDigit[i]==h_s$testDigit[j]){
      temp$count[i]<-h_s$count[j]
    }
  }
}

#create a contingency matrix
data<- data.frame(matrix(0, ncol = clNo, nrow = 10),row.names =0:9)
colnames(data)<-0:(clNo-1)
colnames(data) <- paste("C", colnames(data), sep = "")
for (i in 1:length(h_s$outCluster)) {
  v<-paste("C",as.character(h_s$outCluster[i]),sep="")
  data[h_s$testDigit[i]+1,v]<-h_s$count[i]
}

#PLOT cluster distribution with residuals
data2<-data
colnames(data2)<-0:(clNo-1)
margin.table(data2, margin=1)
margin.table(data2, margin=2)
mosaicplot(data2,off = 50,las=1,shade=TRUE, legend=F, main='Cluster Distribution', xlab='Digits', ylab='Clusters')

#create contingency matrix mosaic graph
ggplot(data = temp, aes(y=outCluster,x=testDigit,  fill=count)) + 
  theme(panel.grid.minor = element_blank(),panel.background = element_rect(fill = "lightgray",
                                                                           colour = "white",
                                                                           size = 0.5, linetype = "solid"),
        panel.grid.major = element_line(size = 0.5, linetype = 'solid',
                                        colour = "white"))+
  geom_tile(colour = "white",
            lwd = 0.5,
            linetype = 1)+ 
  scale_y_continuous(breaks=c(0,5, 10,15,20))+ 
  scale_x_continuous(breaks=seq(0,9,by=1))+
  scale_fill_gradient(low = "lightblue", high = "darkblue") +
  geom_text(aes(label = count), color = "white", size = 2) +
  coord_fixed()


#calculate top cluster and add to the occurence table
ClusterToTestQuot<-seq(0,9,by=1)
TopCluster<-seq(0,9,by=1)
for (i in seq(0,9,by=1)) {
  p<-temp[temp$testDigit==i,]
  p<-p[p$count==max(temp$count[temp$testDigit==i]),]
  #print(p$outCluster)
  TopCluster[i+1]<-p$outCluster[1]
  ClusterToTestQuot[i+1]<-round(p$count[1]/occ$TestCount[occ$Digit==i],3)
}
occ <- data.frame(occ,TopCluster,ClusterToTestQuot)


#mosaic
m<-as.matrix(data2)
colfunc <- colorRampPalette(c("#f5f8a","#0789ed"))
colfunc <- colorRampPalette(c("white","#0789ed"))
image(m,col=colfunc(200),axes=FALSE,main="Contingency matrix",xlab="Digits",ylab="Clusters")
#image.plot(m, col=colfunc(1000),smallplot=c(.85,.86,1,.8))
axis(1, at=c("0.0","0.11","0.22","0.33","0.44","0.55","0.66","0.77","0.88","0.99"), labels=c("0","1","2","3","4","5","6","7","8","9"),xlab="Digits")
axis(2, at=c("0.0","0.2","0.4","0.6","0.8","1.0"), labels=c("Cl0","","","","",paste("Cl",(clNo-1))))
image.plot(m, legend.only=T, col=colfunc(200),smallplot=c(.57,.93,.078,.088),horizontal=T, bigplot=c(.1,.9,.2,.8))
box()

#blue heatmap
m <- t(as.matrix(data2))
colfunc <- colorRampPalette(c("lightblue","blue" ))
heatmap(m, Colv=NA, Rowv=NA,col=colfunc(200))

# Default Heatmap
colfunc <- colorRampPalette(c("black","white" ))
heatmap(m, Colv = NA, Rowv = NA,col=colfunc(50))